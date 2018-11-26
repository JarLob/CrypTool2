package m209;

import common.BestResults;
import common.CtAPI;

public class ReportResult {
    private static long startTimeMillis = System.currentTimeMillis();
    public static boolean simulation = false;
    public static boolean knownPlaintext = false;
    private static int pushed = 0;

    public static void reportResult(int task, int[] roundLayers, int layers, Key key, int currScore, String desc) {
        if (BestResults.shouldPushResult(currScore)) {
            String decryption = key.encryptDecrypt(key.cipher, false);

            long elapsedMillis = System.currentTimeMillis() - ReportResult.startTimeMillis;
            String comment;
            if (simulation) {

                comment = String.format("[%2dL/%3dP][2**%2d][%,5d K/s][%2d: %3d/%2d/%2d/%2d %s] ",
                        key.getCountIncorrectLugs(), key.getCountIncorrectPins(),
                        (long) (Math.log(Key.evaluations) / Math.log(2)),
                        elapsedMillis == 0 ? 0 : Key.evaluations / elapsedMillis,
                        task,
                        roundLayers[0], layers > 1 ? roundLayers[1] : 0, layers > 2 ? roundLayers[2] : 0, layers > 3 ? roundLayers[3] : 0,
                        desc
                );
            } else {
                comment = String.format("[2**%2d][%,5d K/s][%2d: %3d/%2d/%2d/%2d %s] ",
                        (long) (Math.log(Key.evaluations) / Math.log(2)),
                        elapsedMillis == 0 ? 0 : Key.evaluations / elapsedMillis,
                        task,
                        roundLayers[0], layers > 1 ? roundLayers[1] : 0, layers > 2 ? roundLayers[2] : 0, layers > 3 ? roundLayers[3] : 0,
                        desc
                );

            }

            if (BestResults.pushResult(currScore, key.toString(), key.lugs.getLugsString(), decryption, comment)) {
                pushed++;
                if (simulation) {
                    int error = key.getCountIncorrectLugs() * 5 + key.getCountIncorrectPins();
                    CtAPI.displayProgress(Math.max(100 - error, 0), 100);
                } else if (knownPlaintext) {
                    CtAPI.displayProgress(currScore - 120000, 10000);
                } else {
                    CtAPI.displayProgress(currScore, 58000);
                }
            }
        }
        if (currScore == key.originalScore) {
            long elapsedSec = (System.currentTimeMillis() - ReportResult.startTimeMillis)/1000;
            CtAPI.goodbye(0, String.format("Found key - Task %d - %,d decryptions - elapsed %,d seconds - reported %d results\n", task, Key.evaluations, elapsedSec, pushed));
        }
    }

    public static void setOriginalKey(Key simulationKey, EvalType evalType) {
        BestResults.setOriginal(simulationKey.eval(evalType), simulationKey.toString(), simulationKey.toString(), simulationKey.encryptDecrypt(simulationKey.cipher, false),
                "                                                        ");
    }
    public static void setDummyOriginalKeyForCrib(String crib) {
        BestResults.setOriginal(130_000, "", "", crib, "");
    }
    public static void setThreshold(EvalType evalType) {
        switch (evalType) {
            case CRIB:
                BestResults.setScoreThreshold(127_500);
                break;
            case MONO:
                BestResults.setScoreThreshold(40_000);
                break;
        }
    }
}
