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

    public static String readTextFile(String fileName) {

        int[] text = new int[1000000];
        String line = "";
        int len = 0;

        try {
            FileReader fileReader = new FileReader(fileName);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while ((line = bufferedReader.readLine()) != null) {
                for (char c : line.toCharArray()) {
                    int index = getTextSymbol(c);
                    if (index != -1) {
                        text[len] = index;
                        len++;
                    }
                }
            }

            bufferedReader.close();
        } catch (FileNotFoundException ex) {
            CtAPI.goodbye(-1, "Unable to open text file '" + fileName + "'");
        } catch (IOException ex) {
            CtAPI.goodbye(-1, "Error reading text file '" + fileName + "'");
        }

        String cipherStr = getString(Arrays.copyOf(text, len));

        CtAPI.printf("Text file read: %s, length = %d \n%s\n", fileName, len, cipherStr);

        return cipherStr;
    }

    public static int readTextSegmentFromFile(String filename, int from, int[] text) {

        int length = 0;

        try {
            FileReader fileReader = new FileReader(filename);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            int position = 0;
            String line = "";

            while (((line = bufferedReader.readLine()) != null) && (length < text.length)) {
                if (position > from) {
                    for (int i = 0; i < line.length(); i++) {
                        int index = getTextSymbol(line.charAt(i));
                        if ((index != -1) && (length < text.length)) {
                            text[length] = index;
                            length++;
                        }
                    }
                }
                position += line.length();
            }

            bufferedReader.close();
        } catch (FileNotFoundException ex) {
            CtAPI.goodbye(-1, "Unable to open file '" + filename + "'");
        } catch (IOException ex) {
            CtAPI.goodbye(-1, "Unable to read file '" + filename + "'");
        }
        CtAPI.printf("Read segment from file: %s, Position: %d , Length: %d\n", filename, from, length);
        CtAPI.printf("%s\n\n", getString(text));

        return length;
    }

    public static String getString(int[] text) {
        return getString(text, text.length);
    }

    public static String getString(int[] text, int length) {
        StringBuilder m = new StringBuilder();
        for (int i = 0; i < Math.min(text.length, length); i++) {
            m.append(getTextChar(text[i]));
        }
        return m.toString();
    }

    private static long startTime = System.currentTimeMillis();
    public static long getElapsedMillis() {
        return System.currentTimeMillis() - startTime + 1;
    }

    private static Random random = new Random(startTime);
    public static int randomNextInt(int range) {
        return random.nextInt(range);
    }
    public static double randomNextDouble() {
        return random.nextDouble();
    }

}
