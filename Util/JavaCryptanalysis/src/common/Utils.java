package common;


import java.io.*;
import java.util.Arrays;
import java.util.Random;

import static common.CtAPI.printf;

public class Utils {

    public static final String HEXA_FILE = "hexa.bin";
    public static final String BOOK_FILE = "book.txt";

    public static final int A = getTextSymbol('A');
    public static final int X = getTextSymbol('X');
    public static final int Z = getTextSymbol('Z');
    public static final int J = getTextSymbol('J');
    public static final int I = getTextSymbol('I');
    public static final int K = getTextSymbol('K');

    static final String TEXT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    static final int TEXT_ALPHABET_SIZE = TEXT_ALPHABET.length();

    private static int[][] perms6 = createPerms6();

    public static int getTextSymbol(char c) {

        if (c >= 'a' && c <= 'z') {
            return c - 'a';
        }
        if (c >= 'A' && c <= 'Z') {
            return c - 'A';
        }
        return -1;
    }

    public static char getTextChar(int symbol) {

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
                //continue;
            }
            text[len++] = c;
        }
        return Arrays.copyOf(text, len);
    }

    private static String from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".toUpperCase();
    private static String to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".toUpperCase();

    public static String readTextFile(String fileName) {

        int[] text = new int[1000000];
        String line = "";
        int len = 0;

        try {
            FileReader fileReader = new FileReader(fileName);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while ((line = bufferedReader.readLine()) != null) {
                for (char c : line.toCharArray()) {
                    int rep = from.indexOf(c);
                    if (rep != -1) {
                        c = to.charAt(rep);
                    }
                    int index = getTextSymbol(c);
                    if (index != -1) {
                        text[len] = index;
                        len++;
                    }
                }
            }

            bufferedReader.close();
        } catch (FileNotFoundException ex) {
            CtAPI.goodbyeError("Unable to open text file '" + fileName + "'");
        } catch (IOException ex) {
            CtAPI.goodbyeError("Error reading text file '" + fileName + "'");
        }

        String cipherStr = getString(Arrays.copyOf(text, len));

        printf("Text file read: %s, length = %d \n%s\n", fileName, len, cipherStr);

        return cipherStr;
    }

    public static int readTextSegmentFromFile(String filename, int startPost, int[] text) {

        int length = 0;

        try {
            FileReader fileReader = new FileReader(filename);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            int position = 0;
            String line = "";

            while (((line = bufferedReader.readLine()) != null) && (length < text.length)) {
                if (position > startPost) {
                    for (int i = 0; i < line.length(); i++) {
                        char c = line.charAt(i);
                        int rep = from.indexOf(c);
                        if (rep != -1) {
                            c = to.charAt(rep);
                        }
                        int index = getTextSymbol(c);
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
            CtAPI.goodbyeError("Unable to open file '" + filename + "'");
        } catch (IOException ex) {
            CtAPI.goodbyeError("Unable to read file '" + filename + "'");
        }
        printf("Read segment from file: %s, Position: %d , Length: %d\n", filename, startPost, length);
        printf("%s\n\n", getString(text));

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

    public static Random random = new Random(startTime);
    public static int randomNextInt(int range) {
        return random.nextInt(range);
    }
    public static int randomNextInt() {
        return random.nextInt();
    }
    public static double randomNextDouble() {
        return random.nextDouble();
    }

    public static int sum(int[] a) {
        int sum = 0;
        for (int i : a) {
            sum += i;
        }
        return sum;
    }

    public static long sum(long[] a) {
        long sum = 0;
        for (long i : a) {
            sum += i;
        }
        return sum;
    }

    public static boolean in(int x, int... a) {
        for (int i : a) {
            if (i == x) {
                return true;
            }
        }
        return false;
    }

    public static int[] randomPerm6(){
        return perms6[random.nextInt(perms6.length)];
    }

    private static int[][] createPerms6() {
        int[][] perms6 = new int[6 * 5 * 4 * 3 * 2 * 1][6];
        int index = 0;
        for (int i0 = 0; i0 < 6; i0++) {
            for (int i1 = 0; i1 < 6; i1++) {
                if (i1 == i0) {
                    continue;
                }
                for (int i2 = 0; i2 < 6; i2++) {
                    if (i2 == i0 || i2 == i1) {
                        continue;
                    }
                    for (int i3 = 0; i3 < 6; i3++) {
                        if (i3 == i0 || i3 == i1 || i3 == i2) {
                            continue;
                        }
                        for (int i4 = 0; i4 < 6; i4++) {
                            if (i4 == i0 || i4 == i1 || i4 == i2 || i4 == i3) {
                                continue;
                            }
                            for (int i5 = 0; i5 < 6; i5++) {
                                if (i5 == i0 || i5 == i1 || i5 == i2 || i5 == i3 || i5 == i4) {
                                    continue;
                                }

                                perms6[index][0] = i0;
                                perms6[index][1] = i1;
                                perms6[index][2] = i2;
                                perms6[index][3] = i3;
                                perms6[index][4] = i4;
                                perms6[index][5] = i5;
                                index++;

                            }
                        }
                    }
                }
            }
        }
        return perms6;
    }

    public static String readPlaintextSegmentFromFile(String dirname, Language language, int from, int requiredLength, boolean m209) {
        String filename = null;
        switch (language) {
            case ENGLISH:
                filename = "book.txt";
                break;
            case FRENCH:
                filename = "frenchbook.txt";
                break;
            case ITALIAN:
                filename = "italianbook.txt";
                break;
            case GERMAN:
                filename = "germanbook.txt";
                break;
        }
        return readPlaintextSegmentFromFile(dirname + "/" + filename, from, requiredLength, m209);
    }
    // read a plain text segment from a file at a given position and length
    private static String readPlaintextSegmentFromFile(String fileName, int startPosition, int requiredLength, boolean m209) {

        StringBuilder text = new StringBuilder();
        String line;
        int position = 0;
        int fileLength = (int) (new File(fileName).length());
        if (fileLength == 0) {
            CtAPI.goodbyeError("Cannot open file " + fileName);
        }
        if (startPosition < 0) {
            startPosition = randomNextInt(80 * fileLength / 100);
        }

        try {
            // FileReader reads text files in the default encoding.
            FileReader fileReader = new FileReader(fileName);

            // Always wrap FileReader in BufferedReader.
            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while (((line = bufferedReader.readLine()) != null) && (text.length() < requiredLength)) {
                line += " ";
                if (position > startPosition) {
                    //System.out.println(line);
                    line = line.toUpperCase();
                    for (int i = 0; (i < line.length()) && (text.length()  < requiredLength); i++) {
                        char c = line.charAt(i);
                        int rep = from.indexOf(c);
                        if (rep != -1) {
                            c = to.charAt(rep);
                        }
                        if (getTextSymbol(c) == -1) {
                            if (m209) {
                                if (text.length() > 0 && text.charAt(text.length() - 1) == 'Z') {
                                    continue;
                                }
                                c = 'Z';
                            } else {
                                continue;
                            }
                        }
                        text.append(c);
                    }
                }
                position += line.length();
            }

            // Always close files.
            bufferedReader.close();
        } catch (FileNotFoundException ex) {
            CtAPI.goodbyeError("Cannot open book file '" + fileName + "'");
        } catch (IOException ex) {
            CtAPI.goodbyeError("Cannot read book file '" + fileName + "'");
        }

        printf("Generated Random Plaintext - Book: %s, Position: %d , Length: %d\n", fileName, startPosition, text.length());
        printf("%s\n\n", text.toString().replaceAll("Z"," "));


        return text.toString();
    }
}
