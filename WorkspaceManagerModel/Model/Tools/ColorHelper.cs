﻿/*                              
   Copyright 2010 Nils Kopal

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Numerics;
using System.Windows.Media;
using System.IO;
using Cryptool.PluginBase;

namespace WorkspaceManager.Model
{
    public static class ColorHelper
    {
        public static Color AsymmetricColor { 
            get
            { 
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.AsymmetricColor);
            }
            set 
            { 
                WorkspaceManagerModel.Properties.Settings.Default.AsymmetricColor = MediaToDrawing(value);             
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color ClassicColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.ClassicColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.ClassicColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color SymmetricColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.SymmetricColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.SymmetricColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color ToolsColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.ToolsColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.ToolsColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color HashColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.HashColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.HashColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color AnalysisGenericColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.AnalysisGenericColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.AnalysisGenericColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color AnalysisSpecificColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.AnalysisSpecificColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.AnalysisSpecificColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color IntegerColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.IntegerColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.IntegerColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }
 
        public static Color ByteColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.ByteColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.ByteColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color DoubleColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.DoubleColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.DoubleColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color BoolColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.BoolColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.BoolColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color StreamColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.StreamColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.StreamColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color StringColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.StringColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.StringColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color ObjectColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.ObjectColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.ObjectColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color BigIntegerColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.BigIntegerColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.BigIntegerColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color SteganographyColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.SteganographyColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.SteganographyColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color ProtocolColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.ProtocolColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.ProtocolColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }

        public static Color DefaultColor
        {
            get
            {
                return DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.DefaultColor);
            }
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.DefaultColor = MediaToDrawing(value);
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
        }
        
        public static System.Windows.Media.Color DrawingToMedia(System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static System.Drawing.Color MediaToDrawing(System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
      
        /// <summary>
        /// Set colors to default values
        /// </summary>
        public static void SetDefaultColors(){            
            WorkspaceManagerModel.Properties.Settings.Default.Reset();
        }

        /// <summary>
        /// Returns a Color for a given Type
        /// 
        /// example:
        ///     System.String -> Colors.LightGray;
        ///     
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Color GetColor(Type type)
        {
            try
            {
                if (type == null)
                {
                    return DefaultColor;
                }

                ComponentCategoryAttribute[] attr = type.GetComponentCategoryAttributes();
                if (attr == null || attr.Length == 0)
                    return DefaultColor;

                switch (attr[0].Category) // consider first attribute found, ignore remaining ones
                {
                    case ComponentCategory.CiphersClassic:
                        return ClassicColor;
                    case ComponentCategory.CiphersModernSymmetric:
                        return SymmetricColor;
                    case ComponentCategory.CiphersModernAsymmetric:
                        return AsymmetricColor;
                    case ComponentCategory.Steganography:
                        return SteganographyColor;
                    case ComponentCategory.HashFunctions:
                        return HashColor;
                    case ComponentCategory.CryptanalysisSpecific:
                        return AnalysisSpecificColor;
                    case ComponentCategory.CryptanalysisGeneric:
                        return AnalysisGenericColor;
                    case ComponentCategory.Protocols:
                        return ProtocolColor;
                    case ComponentCategory.ToolsStandalone:
                    case ComponentCategory.ToolsBoolean:
                    case ComponentCategory.ToolsDataflow:
                    case ComponentCategory.ToolsDataInputOutput:
                    case ComponentCategory.ToolsMisc:
                    case ComponentCategory.ToolsP2P:
                        return ToolsColor;
                    default:
                        return DefaultColor;
                }
            }
            catch (Exception)
            {
                return Colors.Black;
            }
            
        }

        /// <summary>
        /// Get a color for a ConnectorModel (drawed as line)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Color GetLineColor(Type type)
        {
            try
            {
                if (typeof(int).Equals(type) || typeof(int[]).Equals(type)) return IntegerColor;
                if (typeof(byte[]).Equals(type) || typeof(byte[]).Equals(type)) return ByteColor;
                if (typeof(double).Equals(type) || typeof(double[]).Equals(type)) return DoubleColor;
                if (typeof(bool).Equals(type) || typeof(bool[]).Equals(type)) return BoolColor;

                if (typeof(Stream).Equals(type)) return StreamColor;
                if (typeof(string).Equals(type) || typeof(string[]).Equals(type)) return StringColor;

                if (typeof(object).Equals(type)) return ObjectColor;
                if (typeof(BigInteger).Equals(type)) return BigIntegerColor;
                return DefaultColor;
            }
            catch (Exception)
            {
                return Colors.Black;
            }
        }

        public static Color GetColorLight(Type type)
        {
            Color clr = ColorHelper.GetColor(type);
            System.Drawing.Color clr2 = System.Windows.Forms.ControlPaint.Light(System.Windows.Forms.ControlPaint.LightLight(System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B)));
            return Color.FromArgb(clr2.A, clr2.R, clr2.G, clr2.B);
        }
    }
}
