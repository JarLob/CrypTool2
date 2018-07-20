package playfair;

import common.BestResults;
import common.CtAPI;
import common.Utils;
import common.SimulatedAnnealing;

import java.util.Random;

public class SolvePlayfair {
    
    private static long startTime = System.currentTimeMillis();
    private static long evaluations = 0;

    static void solve(int taskNumber, int saCycles, int innerRounds /* 200000 */, int multiplier/*1500*/, int[] cipherText, String crib, Key simulationKey) {

        long simulationOriginalScore = (simulationKey == null) ? 0 : simulationKey.score;
        Key currentKey = new Key();
        Key newKey = new Key();
        currentKey.setCipher(cipherText);
        newKey.setCipher(cipherText);
        currentKey.setCrib(crib);
        newKey.setCrib(crib);

        long serialCounter = 0;

        for (int cycle = 0; cycle < saCycles || saCycles == 0; cycle++) {

            if (taskNumber == 0) {
                CtAPI.updateProgress(cycle, saCycles);
            }

            Transformations.randomize();
            currentKey.random();

            long currentScore = currentKey.eval();

            for (int innerRound = 0; innerRound < innerRounds; innerRound++) {
                Transformations.apply(currentKey, newKey, serialCounter++);

                evaluations++;

                long newScore = newKey.eval();

                if (SimulatedAnnealing.acceptHexaScore(newScore, currentScore, multiplier)) {
                    currentKey.copy(newKey);
                    currentScore = newScore;

                    if (BestResults.shouldPushResult(newScore)) {
                        long elapsed = System.currentTimeMillis() - startTime + 1;
                        BestResults.pushResult(newScore,
                                newKey.toString(),
                                Utils.getString(newKey.fullDecryption),
                                String.format("[%d/%d}[%,d sec.][%,dK (%,dK/sec.][Task: %2d][Mult.: %,d]",
                                        newKey.decryptionRemoveNullsLength, cipherText.length, elapsed / 1000, evaluations / 1000, evaluations / elapsed, taskNumber, multiplier));
                    }
                    if (currentScore == simulationOriginalScore || newKey.matchesFullCrib()) {
                        CtAPI.goodbye(0, "Found key");
                    }
                }
            }
        }
    }

    static void solveMultithreaded(int[] cipherText, String cribString, int threads, int cycles, Key simulationKey) {
        Random r = new Random();
        final Key simulationKey_ = simulationKey;
        for (int t_ = 0; t_ < threads; t_++) {
            final int t = t_;
            double factor = 0.5 + r.nextDouble();
            factor = 1.0;
            int multiplier = (int) (factor* 150_000)/cipherText.length;
            new Thread(
                    new Runnable() {
                        @Override
                        public void run() {
                            solve(t, cycles, 200_000, multiplier, cipherText, cribString, simulationKey_);
                        }
                    }
            ).start();
        }
    }
}
