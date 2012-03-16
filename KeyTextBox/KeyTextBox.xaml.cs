﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace KeyTextBox
{
    /// <summary>
    /// Interaction logic for KeyTextBox.xaml
    /// </summary>
    public partial class KeyTextBox : UserControl
    {
        private Run _keyRun;
        
        public static readonly DependencyProperty KeyManagerProperty =
            DependencyProperty.Register(
            "KeyManager",
            typeof(IKeyManager),
            typeof(KeyTextBox),
            new FrameworkPropertyMetadata());
        
        public IKeyManager KeyManager
        {
            get
            {
                return (IKeyManager)GetValue(KeyManagerProperty);
            }
            set
            {
                if (KeyManager != null)
                {
                    KeyManager.OnKeyChanged -= KeyManagerChanged;
                }

                SetValue(KeyManagerProperty, value);
                
                if (value != null)
                {
                    SetKeyBox(value.GetKey(), 0);
                    value.OnKeyChanged += KeyManagerChanged;
                }
            }
        }

        public static readonly DependencyProperty CurrentKeyProperty =
            DependencyProperty.Register(
            "CurrentKey",
            typeof(string),
            typeof(KeyTextBox),
            new FrameworkPropertyMetadata());


        public string CurrentKey
        {
            get
            {
                return (string)GetValue(CurrentKeyProperty);
            }
            set
            {
                SetValue(CurrentKeyProperty, value);
                KeyManager.SetKey(value);
            }
        }
        
        public KeyTextBox()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(KeyBox, PastingHandler);
        }

        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            var cb = e.DataObject.GetData(typeof (string)) as string;
            HandleInput(cb);
            e.CancelCommand();
            e.Handled = true;
        }

        private void KeyBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            HandleInput(e.Text);
        }

        private void HandleInput(string input)
        {
            foreach (var inChar in input)
            {
                List<char> possibleChars;
                var key = KeyManager.GetKey();
                int caretIndex;
                var caretPosition = KeyBox.CaretPosition;
                var next = false;

                do
                {
                    if (caretPosition == null)
                    {
                        return;
                    }

                    caretIndex = GetKeyOffset(caretPosition);
                    possibleChars = GetPossibleCharactersAtKeyEnd(key.Substring(0, caretIndex));
                    if (possibleChars == null || possibleChars.Count <= 1)
                    {
                        if (caretPosition.GetNextInsertionPosition(LogicalDirection.Forward) == null)
                        {
                            return;
                        }

                        caretPosition = caretPosition.GetNextInsertionPosition(LogicalDirection.Forward);
                        if (possibleChars != null && possibleChars.Count == 1 && possibleChars.Contains(inChar))
                        {
                            next = true;
                        }
                        possibleChars = null;
                    }
                } while (!next && (possibleChars == null || possibleChars.Count <= 1));

                if (!next)
                {
                    //If the current position allows multiple options:
                    if (ReplaceCharInKey(key, possibleChars, inChar, caretIndex))
                        return;
                }
                else
                {
                    KeyBox.CaretPosition = caretPosition;
                }
            }
        }

        private void KeyBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var key = KeyManager.GetKey();
            switch (e.Key)
            {
                case Key.Back:
                case Key.Delete:
                    e.Handled = true;
                    var caretIndex = GetKeyOffset(KeyBox.CaretPosition);
                    if (e.Key == Key.Back)
                    {
                        caretIndex = (caretIndex == 0) ? 0 : (caretIndex - 1);
                    }
                    ReplaceCharInKey(key, null, '*', caretIndex);
                    caretIndex = GetKeyOffset(KeyBox.CaretPosition);
                    SetKeyOffset(caretIndex-1);
                    break;
            }
        }

        private bool ReplaceCharInKey(string key, List<char> possibleChars, char inChar, int caretIndex)
        {
            int startPosition;
            int endPosition;
            var elType = GetElementType(key, caretIndex, out startPosition, out endPosition);

            switch (inChar)
            {
                case '[':
                    switch (elType)
                    {
                        case ElementType.Joker:
                        case ElementType.Character:
                            key = key.Remove(caretIndex, 1).Insert(caretIndex, "[]");
                            caretIndex++;
                            break;
                        case ElementType.Group:
                            caretIndex = startPosition + 1;
                            break;
                    }
                    break;
                case ']':
                    if (elType == ElementType.Group)
                    {
                        caretIndex = endPosition + 1;
                    }
                    break;
                case '*':
                    switch (elType)
                    {
                        case ElementType.Joker:
                        case ElementType.Character:
                            key = key.Remove(caretIndex, 1).Insert(caretIndex, "*");
                            caretIndex++;
                            break;
                        case ElementType.Group:
                            key = key.Remove(startPosition, endPosition - startPosition + 1).Insert(startPosition, "*");
                            caretIndex = startPosition + 1;
                            break;
                    }
                    break;
                default:
                    if (possibleChars.Contains(inChar))
                    {
                        switch (elType)
                        {
                            case ElementType.Joker:
                            case ElementType.Character:
                                key = key.Remove(caretIndex, 1).Insert(caretIndex, inChar.ToString());
                                caretIndex++;
                                break;
                            case ElementType.Group:
                                var p = Math.Max(caretIndex, startPosition + 1);
                                key = key.Insert(p, inChar.ToString());
                                caretIndex = p + 1;
                                break;
                        }
                    }
                    else if (inChar == '-')
                    {
                        if (elType == ElementType.Group && caretIndex > startPosition && caretIndex <= endPosition)
                        {
                            key = key.Insert(caretIndex, inChar.ToString());
                            caretIndex++;
                        }
                    }
                    else
                    {
                        return true;
                    }
                    break;
            }

            KeyManager.SetKey(key);
            SetKeyBox(key, caretIndex);
            return false;
        }

        private int GetKeyOffset(TextPointer caretPosition)
        {
            var count = 0;
            foreach (var inline in caretPosition.Paragraph.Inlines)
            {
                if (inline != caretPosition.Parent)
                {
                    count += ((Run) inline).Text.Length;
                }
                else
                {
                    count += caretPosition.GetTextRunLength(LogicalDirection.Backward);
                    return count;
                }
            }
            return 0;
        }


        private void SetKeyOffset(int caretPosition)
        {
            var count = 0;
            foreach (var inline in KeyBox.CaretPosition.Paragraph.Inlines)
            {
                var run = (Run) inline;
                if (count+run.Text.Length < caretPosition)
                {
                    count += run.Text.Length;
                }
                else
                {
                    var caret = run.ContentStart;
                    for (; count < caretPosition; count++)
                    {
                        caret = caret.GetNextInsertionPosition(LogicalDirection.Forward);
                    }
                    KeyBox.CaretPosition = caret;
                    return;
                }
            }
        }

        private void SetKeyBox(string key, int caretIndex)
        {
            var paragraph = new Paragraph();
            paragraph.TextAlignment = TextAlignment.Left;
            var kcount = 0;

            while (kcount < key.Length)
            {
                if (key[kcount] == '[')
                {
                    int start = kcount;
                    var invalidGroup = CheckGroup(key, ref kcount);
                    kcount++;
                    var run = new Run(key.Substring(start, kcount - start));
                    run.Background = invalidGroup ? Brushes.DarkRed : Brushes.DarkKhaki;
                    paragraph.Inlines.Add(run);
                }
                else
                {
                    paragraph.Inlines.Add(new Run(key[kcount].ToString()));
                    kcount++;
                }
            }
            KeyBox.Document = new FlowDocument(paragraph);
            SetKeyOffset(caretIndex);
        }

        private void KeyManagerChanged(string key)
        {
            SetValue(CurrentKeyProperty, key);
            SetKeyBox(key, 0);
        }

        private static bool CheckGroup(string key, ref int kcount)
        {
            int state = 0;
            bool invalidGroup = false;
            for (kcount++; key[kcount] != ']'; kcount++)
            {
                if (!invalidGroup)
                {
                    if (key[kcount] == '-')
                    {
                        if (state != 1)
                        {
                            invalidGroup = true;
                        }
                        else
                        {
                            state = 2;
                        }
                    }
                    else
                    {
                        state = state == 2 ? 3 : 1;
                    }
                }
            }

            if (state == 2 || state == 0)
            {
                invalidGroup = true;
            }
            return invalidGroup;
        }

        private enum ElementType {Joker, Character, Group}

        private ElementType GetElementType(string key, int position, out int startPosition, out int endPosition)
        {
            int kcount = 0;
            
            while (kcount < position)
            {
                if (key[kcount] == '[')
                {
                    if (GetGroupInformations(key, position, out startPosition, out endPosition, ref kcount)) 
                        return ElementType.Group;
                }
                else
                {
                    kcount++;
                }
            }

            startPosition = kcount;
            endPosition = kcount;
            if (key[kcount] == '*')
            {
                return ElementType.Joker;
            }
            else if (key[kcount] == '[')
            {
                GetGroupInformations(key, position, out startPosition, out endPosition, ref kcount);
                return ElementType.Group;
            }
            else
            {
                return ElementType.Character;
            }
        }

        private bool GetGroupInformations(string key, int position, out int startPosition, out int endPosition,
                                                 ref int kcount)
        {
            startPosition = kcount;
            do
            {
                kcount++;
            } while (key[kcount] != ']');
            endPosition = kcount;

            if (kcount >= position)
            {
                return true;
            }
            return false;
        }

        private List<char> GetPossibleCharactersAtKeyEnd(string key)
        {
            var format = KeyManager.GetFormat();
            int fcount = 0;
            int kcount = 0;

            while (kcount < key.Length)
            {
                if (key[kcount] == '[')
                {
                    do
                    {
                        kcount++;
                        if (kcount == key.Length)
                        {
                            return FormatHelper.GetPossibleCharactersFromFormat(format, fcount);
                        }
                    } while (key[kcount] != ']');
                    kcount++;
                    fcount = FormatHelper.GetNextFormatIndex(format, fcount);
                }
                else
                {
                    kcount++;
                    fcount = FormatHelper.GetNextFormatIndex(format, fcount);
                }
            }

            return FormatHelper.GetPossibleCharactersFromFormat(format, fcount);
        }
    }
}
