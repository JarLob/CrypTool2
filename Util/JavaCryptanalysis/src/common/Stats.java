package common;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ShortBuffer;
import java.nio.channels.FileChannel;


public class Stats {





    private static short[] hexagramStatsShort = null;

    public static boolean readHexaStatsFile(String filename) {
        long start = System.currentTimeMillis();

        CtAPI.printf("Loading hexagram stats file %s (%,d free bytes before loading)\n",
                filename, Runtime.getRuntime().freeMemory());

        int totalShortRead = 0;

        try {
            FileInputStream is = new FileInputStream(new File(filename));

            hexagramStatsShort = new short[ 26 * 26 * 26 * 26 * 26 * 26];

            final int CHUNK = 65536;

            short[] hexagramStatsBuffer = new short[CHUNK];
            byte[] bytes = new byte[CHUNK * 2];

            int read;
            while ((read = is.read(bytes)) > 0) {
                ByteBuffer myByteBuffer = ByteBuffer.wrap(bytes);
                ShortBuffer myShortBuffer = myByteBuffer.asShortBuffer();
                myShortBuffer.get(hexagramStatsBuffer);
                System.arraycopy(hexagramStatsBuffer, 0, hexagramStatsShort, totalShortRead, read / 2);
                totalShortRead += read/2;
            }
            is.close();


        } catch (IOException e) {
            e.printStackTrace();
            return false;
        }
        CtAPI.printf("Hexagram stats file %s loaded successfully (%f seconds), size = %,d bytes (%,d free bytes after loading)\n",
                filename, (System.currentTimeMillis() - start) / 1_000.0, totalShortRead * 2, Runtime.getRuntime().freeMemory());
        CtAPI.println("");
        CtAPI.println("");
        return true;
    }

    private final static int MOD = 26 * 26 * 26 * 26 * 26;

    public static long evalPlaintextHexagram(int[] plaintext, int plaintextLength) {

        int index = (((((((plaintext[0] * 26) + plaintext[1]) * 26) + plaintext[2]) * 26) + plaintext[3]) * 26 + plaintext[4]);
        long val = 0;
        for (int i = 5; i < plaintextLength; i++) {
            index = (index % MOD) *  26 + plaintext[i];
            val += hexagramStatsShort[index];
        }
        return (val * 1000)/ (plaintextLength - 5);

    }


    public static long evalPlaintextHexagram(int[] plaintext) {
        CtAPI.shutdownIfNeeded();
        return evalPlaintextHexagram(plaintext, plaintext.length);
    }
}


