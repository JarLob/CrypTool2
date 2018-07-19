package common;


import common.CtAPI;

import java.io.*;
import java.util.Arrays;
import java.util.Random;

public class Utils {

    public static final String HEXA_FILE = "hexa.bin";
    public static final String BOOK_FILE = "book.txt";

    public static final int X = getTextSymbol('X');
    public static final int Z = getTextSymbol('Z');
    public static final int J = getTextSymbol('J');
    public static final int I = getTextSymbol('I');
    public static final int K = getTextSymbol('K');


    private static final String TEXT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static final int TEXT_ALPHABET_SIZE = TEXT_ALPHABET.length();

    public static int getTextSymbol(char c) {

        if (c >= 'a' && c <= 'z') {
            return c - 'a';
        }
        if (c >= 'A' && c <= 'Z') {
            return c - 'A';
        }
        return -1;
    }

    private static char getTextChar(int symbol) {

        if ((symbol >= 0) && (symbol <= (TEXT_ALPHABET_SIZE - 1)))
            return (TEXT_ALPHABET.charAt(symbol));
        else
            return '?';

    }

    public static int[] getText(String textString) {
        int[] text = new int[textString.length()];
        int len = 0;
        for (int i = 0; i < textString.length(); i++) {
            int c = getTextSymbol(textString.charAt(i));
            if (c == -1) {
                continue;
            }
            text[len++] = c;
        }
        return Arrays.copyOf(text, len);
    }

    // read a cipher text from file. Only characters from the alphabet are read
    public static String readCipherFile(String fileName) {

        int[] ciphertext = new int[1000000];
        String line = "";
        int len = 0;

        try {
            FileReader fileReader = new FileReader(fileName);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while ((line = bufferedReader.readLine()) != null) {
                for (char c : line.toCharArray()) {
                    int index = getTextSymbol(c);
                    if (index != -1) {
                        ciphertext[len] = index;
                        len++;
                    }
                }
            }

            bufferedReader.close();
        } catch (FileNotFoundException ex) {
            CtAPI.goodbye(-1, "Unable to open ciphertext file '" + fileName + "'");
        } catch (IOException ex) {
            CtAPI.goodbye(-1, "Error reading ciphertext file '" + fileName + "'");
        }

        String cipherStr = getString(Arrays.copyOf(ciphertext, len));


        CtAPI.printf("Ciphertext file read: %s, length = %d \n%s\n", fileName, len, cipherStr);

        return cipherStr;
    }

    // read a plain text segment from a file at a given position and length
    public static int readPlaintextSegmentFromFile(String fileName, int from, int[] plaintext) {

        String line = "";
        int length = 0;
        int position = 0;

        try {
            FileReader fileReader = new FileReader(fileName);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while (((line = bufferedReader.readLine()) != null) && (length < plaintext.length)) {
                if (position > from) {
                    for (int i = 0; i < line.length(); i++) {
                        int index = getTextSymbol(line.charAt(i));
                        if ((index != -1) && (length < plaintext.length)) {
                            plaintext[length] = index;
                            length++;
                        }
                    }
                }
                position += line.length();
            }

            bufferedReader.close();
        } catch (FileNotFoundException ex) {
            CtAPI.println("Unable to open book file '" + fileName + "'");
            CtAPI.goodbye(-1, "Cannot open book file " + fileName);
        } catch (IOException ex) {
            CtAPI.goodbye(-1, "Cannot read book file " + fileName);
        }
        CtAPI.printf("Generated Random Plaintext - Book: %s, Position: %d , Length: %d\n", fileName, from, length);
        CtAPI.printf("%s\n\n", getString(plaintext));


        return length;
    }


    public static String getString(int[] text) {

        String m = "";
        for (int t : text) {
            m += getTextChar(t);
        }

        return m;
    }
    public static String getString(int[] text, int length) {

        String m = "";
        for (int i = 0; i < Math.min(text.length, length); i++) {
            m += getTextChar(text[i]);
        }

        return m;
    }

    public static Random random = new Random(System.currentTimeMillis());

    // generate a random number from 0 to (range-1)
    public static int randomGet(int range) {
        return random.nextInt(range);

    }





}
