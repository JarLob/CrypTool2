package playfair;

import common.BestList;
import common.CtAPI;
import common.Utils;
import common.SimulatedAnnealing;

import java.util.concurrent.locks.ReentrantLock;

public class SolvePlayfair {
    
    private static long startTime = System.currentTimeMillis();
    private static long evaluations = 0;
    private static long bestScoreOverall = 0;
    private static ReentrantLock lock = new ReentrantLock();

    static void solveSA(int taskNumber, int saCycles, int innerRounds /* 200000 */, int multiplier/*1500*/, int[] cipherText, String crib, Key simulationKey) {

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
                if (saCycles == 0) {
                    CtAPI.updateProgress(1 + (cycle % 95), 100);
                } else {
                    CtAPI.updateProgress(cycle, saCycles);
                }
            }

            Transform.randomize();

            currentKey.random();

            long currentScore = currentKey.eval();

            for (int innerRound = 0; innerRound < innerRounds; innerRound++) {
                Transform.applyTransformation(currentKey, newKey, serialCounter++);

                evaluations++;

                long newScore = newKey.eval();

                long elapsed = System.currentTimeMillis() - startTime + 1;

                if (SimulatedAnnealing.acceptHexaScore(newScore, currentScore, multiplier)) {
                    currentKey.copy(newKey);
                    currentScore = newScore;

                    if (BestList.shouldInsert(newScore)) {
                        BestList.insert(newScore,
                                newKey.toString(),
                                Utils.getString(newKey.fullDecryption),
                                String.format("[%d/%d}[%,d sec.][%,dK (%,dK/sec.][Task: %2d][Mult.: %,d]",
                                        newKey.decryptionRemoveNullsLength, cipherText.length, elapsed / 1000, evaluations/1000, evaluations / elapsed, taskNumber, multiplier));
                    }
                    if (currentScore > bestScoreOverall) {
                        boolean matchesFullCrib = newKey.matchesFullCrib();
                        lock.lock();
                        if (currentScore > bestScoreOverall) {
                            bestScoreOverall = currentScore;
                        }
                        if (currentScore == simulationOriginalScore || matchesFullCrib) {
                            CtAPI.print("Found key ...\n");
                            CtAPI.goodbye(0, "Found key");
                        }
                        lock.unlock();
                    }
                }
            }
        }
    }

}
