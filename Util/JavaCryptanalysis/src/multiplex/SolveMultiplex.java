package multiplex;

import common.BestResults;
import common.CtAPI;
import common.SimulatedAnnealing;
import common.Utils;

class SolveMultiplex {

    static long cycle(long bestOverall, Multiplex multiplex, Multiplex realMultiplex, long realMultiplexScore, int innerRounds, int multiplier, int saCycle) {

        multiplex.randomizeKey();
        long bestVal = multiplex.eval();
        for (int rounds = 0; rounds < innerRounds; rounds++) {
            int randomShift = Utils.randomGet(multiplex.NUMBER_OF_STRIPS);
            for (int i = 0; i < multiplex.NUMBER_OF_STRIPS; i++) {
                int pi = (randomShift + i) % multiplex.NUMBER_OF_STRIPS;
                for (int j = i + 1; j < multiplex.NUMBER_OF_STRIPS; j++) {
                    int pj = (randomShift + j) % multiplex.NUMBER_OF_STRIPS;
                    if (pi >= multiplex.NUMBER_OF_STRIPS_USED_IN_KEY && pj >= multiplex.NUMBER_OF_STRIPS_USED_IN_KEY) {
                        continue;
                    }
                    //count++;
                    multiplex.swapInKey(pi, pj);
                    long newVal = multiplex.eval();
                    if (SimulatedAnnealing.acceptHexaScore(newVal, bestVal, multiplier)) {
                        if (BestResults.shouldPushResult(newVal)) {
                            BestResults.pushResult(newVal, multiplex.toString(), Utils.getString(multiplex.decryption), String.format("%,5d/%,5d", saCycle, rounds));
                        }
                        bestVal = newVal;
                        if (bestVal > bestOverall) {
                            bestOverall = bestVal;

                            if (bestOverall == realMultiplexScore) {
                                CtAPI.goodbye(0, "Found original encryption key used for simulation");
                            }
                            if (multiplex.matchesFullCrib()) {
                                CtAPI.goodbye(0, "Found full match with crib");
                            }
                        }
                    } else {
                        multiplex.swapInKey(pi, pj);
                    }
                }
            }
        }
        return bestOverall;
    }
}
