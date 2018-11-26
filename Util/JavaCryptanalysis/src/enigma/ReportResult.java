package enigma;

import common.BestResults;
import common.CtAPI;

public class ReportResult {

    static long lastProgressUpdate = System.currentTimeMillis();
    public static synchronized void reportResult(int task, Key key, int currScore, String plaintext, String desc) {
        if (BestResults.shouldPushResult(currScore)) {
            BestResults.pushResult(currScore, key.getKeyStringLong(), key.getKeyStringShort(), plaintext, desc);
        }
    }
    public static synchronized void displayProgress(long count, long max) {
        long now = System.currentTimeMillis();
        if (now > lastProgressUpdate + 100) {
            CtAPI.displayProgress(count, max);
            lastProgressUpdate = now;
        }
    }
}
