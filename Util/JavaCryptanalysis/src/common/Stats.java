package common;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ShortBuffer;


public class Stats {

    private static short[] hexagramStats = null;
    public static long evaluations = 0;

    public static boolean readHexagramStatsFile(String filename) {
        long start = System.currentTimeMillis();

        CtAPI.printf("Loading hexagram stats file %s (%,d free bytes before loading)\n",
                filename, Runtime.getRuntime().freeMemory());

        int totalShortRead = 0;

        try {
            FileInputStream is = new FileInputStream(new File(filename));

            hexagramStats = new short[26 * 26 * 26 * 26 * 26 * 26];

            final int CHUNK_SIZE = 65536;

            short[] hexagramStatsBuffer = new short[CHUNK_SIZE];
            byte[] bytes = new byte[CHUNK_SIZE * 2];

            int read;
            while ((read = is.read(bytes)) > 0) {
                ByteBuffer myByteBuffer = ByteBuffer.wrap(bytes);
                ShortBuffer myShortBuffer = myByteBuffer.asShortBuffer();
                myShortBuffer.get(hexagramStatsBuffer);
                System.arraycopy(hexagramStatsBuffer, 0, hexagramStats, totalShortRead, read / 2);
                totalShortRead += read / 2;
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

    private final static int POWER_26_5 = 26 * 26 * 26 * 26 * 26;

    public static long evalPlaintextHexagram(int[] plaintext, int plaintextLength) {

        Stats.evaluations++;

        int index = (((((((plaintext[0] * 26) + plaintext[1]) * 26) + plaintext[2]) * 26) + plaintext[3]) * 26 + plaintext[4]);
        long val = 0;
        for (int i = 5; i < plaintextLength; i++) {
            index = (index % POWER_26_5) * 26 + plaintext[i];
            val += hexagramStats[index];
        }
        return (val * 1000) / (plaintextLength - 5);

    }


    public static long evalPlaintextHexagram(int[] plaintext) {
        CtAPI.shutdownIfNeeded();
        return evalPlaintextHexagram(plaintext, plaintext.length);
    }

    public static String evaluationsSummary(){
        long elapsed = Utils.getElapsedMillis();
        return String.format("[%,d sec.][%,dK (%,dK/sec.]", elapsed / 1000, Stats.evaluations / 1000, Stats.evaluations / elapsed);
    }
}


