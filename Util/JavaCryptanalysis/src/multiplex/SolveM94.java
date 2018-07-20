package multiplex;

import common.BestResults;
import common.CtAPI;
import common.Runnables;
import common.Utils;

import java.util.concurrent.atomic.AtomicInteger;

class SolveM94 {


    static long sa(M94 m94, M94 realKey, long realMultiplexScore, int saCycles, boolean updateProgress) {

        long bestOverall = 0;
        for (int saCycle = 0; saCycle < saCycles || saCycles == 0; saCycle++) {
            bestOverall = SolveMultiplex.cycle(bestOverall, m94, realKey, realMultiplexScore, 200, 2_000, saCycle);
            if (updateProgress) {
                CtAPI.updateProgress(saCycle, saCycles);
            }
        }

        return bestOverall;
    }

    static void saSweepOffsets(final int[] cipher, final String cribStr, final int saCycles, final int internalSaCycles, final int threads, final M94 realM94) {
        if (cipher.length > 75) {
            CtAPI.goodbye(-1, "With M94 and unknown offsets, search works only for ciphertext with exactly 75 symbols)");
        }
        if (realM94 != null) {
            realM94.setCipherAndCrib(cipher, cribStr);
            BestResults.setOriginal(realM94.eval(), realM94.toString(), Utils.getString(realM94.decryption), "Original");
        }
        AtomicInteger countOffsetsChecked = new AtomicInteger();
        Runnables runnables = new Runnables();
        final  long realMultiplexScore = realM94 == null ? -1 : realM94.eval();
        for (int offset0_ = 0; offset0_ < 26; offset0_++) {
            final int offset0 = offset0_;
            runnables.addRunnable(() -> {
                long best = 0;
                M94 m94 = new M94(3);
                m94.setCipherAndCrib(cipher, cribStr);
                m94.setOffset(0, offset0);
                for (int offset1 = 0; offset1 < 26; offset1++) {
                    m94.setOffset(1, offset1);
                    for (int offset2 = 0; offset2 < 26; offset2++) {
                        m94.setOffset(2, offset2);
                        best = Math.max(best, sa(m94, realM94, realMultiplexScore, internalSaCycles, false));
                    }
                }
                CtAPI.printf("Completed %s - Best %,8d (%s)\n",
                        m94.offsetString(0) + ",..,.. ",
                        best,
                        realM94 == null ? "" : ("Real offsets: " + realM94.offsetString()));
                synchronized (countOffsetsChecked) {
                    CtAPI.updateProgress(countOffsetsChecked.incrementAndGet(), 25);
                }

            });
        }


        for (int saCycle = 0; (saCycle< saCycles) || (saCycles == 0); saCycle++) {
            countOffsetsChecked.set(0);
            CtAPI.updateProgress(countOffsetsChecked.intValue(), 25);

            runnables.run(threads);

        }
    }
}
