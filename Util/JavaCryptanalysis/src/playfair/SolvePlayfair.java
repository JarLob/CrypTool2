package playfair;

import common.*;

import java.util.Random;

public class SolvePlayfair {

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
                CtAPI.displayProgress(cycle, saCycles);
            }

            Transformations.randomize();
            currentKey.random();

            long currentScore = currentKey.eval();

            for (int innerRound = 0; innerRound < innerRounds; innerRound++) {
                Transformations.apply(currentKey, newKey, serialCounter++);

                long newScore = newKey.eval();

                if (SimulatedAnnealing.acceptHexaScore(newScore, currentScore, multiplier)) {
                    currentKey.copy(newKey);
                    currentScore = newScore;

                    if (BestResults.shouldPushResult(newScore)) {
                        BestResults.pushResult(newScore,
                                newKey.toString(),
                                newKey.toString(),
                                Utils.getString(newKey.fullDecryption),
                                Stats.evaluationsSummary() +
                                    String.format("[%d/%d}[Task: %2d][Mult.: %,d]",
                                            newKey.decryptionRemoveNullsLength, cipherText.length, taskNumber, multiplier));
                        if (currentScore == simulationOriginalScore || newKey.matchesFullCrib()) {
                            CtAPI.printf("Key found");
                            CtAPI.goodbye();
                        }
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
            final double factor = (cribString.length() > cipherText.length/2) ? 0.1 : 1.0;
            final int multiplier = (int) (factor* 150_000)/cipherText.length;

            new Thread(
                    () -> solve(t, cycles, 200_000, multiplier, cipherText, cribString, simulationKey_)
            ).start();
        }
    }
}
