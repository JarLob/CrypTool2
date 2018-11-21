package m209;

import common.BestResults;
import common.CtAPI;

public class ReportResult {
    private static long startTimeMillis = System.currentTimeMillis();

    public static void reportResult(int task, int[] roundLayers, int layers, Key key, int currScore, String desc) {
        if (BestResults.shouldPushResult(currScore)) {
            String decryption = key.encryptDecrypt(key.cipher, false);

            long elapsedMillis = System.currentTimeMillis() - ReportResult.startTimeMillis;
            String comment = String.format("[%2dL/%3dP][2**%2d][%,5d K/s][%2d: %3d/%2d/%2d/%2d %s] ",
                    key.getCountIncorrectLugs(), key.getCountIncorrectPins(),
                    (long) (Math.log(Key.evaluations) / Math.log(2)),
                    elapsedMillis == 0 ? 0 : Key.evaluations / elapsedMillis,
                    task,
                    roundLayers[0], layers > 1 ? roundLayers[1] : 0, layers > 2 ? roundLayers[2] : 0, layers > 3 ? roundLayers[3] : 0,
                    desc
                    );

            BestResults.pushResult(currScore, key.toString(), decryption, comment);
        }
        if (currScore == key.originalScore) {
            long elapsedSec = (System.currentTimeMillis() - ReportResult.startTimeMillis)/1000;
            CtAPI.goodbye(0, String.format("Found key - Task %d - %,d decryptions - elapsed %,d seconds\n", task, Key.evaluations, elapsedSec));
        }
    }

    public static void setOriginalKey(Key simulationKey, EvalType evalType) {
        BestResults.setOriginal(simulationKey.eval(evalType), simulationKey.toString(), simulationKey.encryptDecrypt(simulationKey.cipher, false),
                "                                                        ");
    }
    public static void setDummyOriginalKeyForCrib(String crib) {
        BestResults.setOriginal(130000, "", crib, "");
    }
    public static void setThreshold(EvalType evalType) {
        switch (evalType) {
            case CRIB:
                BestResults.setScoreThreshold(127500);
                break;
            case MONO:
                BestResults.setScoreThreshold(40000);
                break;
        }
    }
}
