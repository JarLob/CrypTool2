package multiplex;


import common.BestList;
import common.CtAPI;
import common.Utils;

class SolveM138 {

    /**
     * Solve M138 ciphertext only.
     *
     * @param m138     - Multiplx object. Has the ciphertext already set.
     * @param realM138 - the original settings, if known. otherwise, null.
     * @param offset   -
     * @param saCycles - number of Simulated Annealing cycles. 0 for infinite.
     * @return best score found.
     */
    static long sa(M138 m138, M138 realM138, long realMultiplexScore, int offset, int saCycles) {
        long bestOverall = 0;
        for (int saCycle = 0; saCycle < saCycles || saCycles == 0; saCycle++) {
            if (offset == 0) {
                m138.setOffset(saCycle % m138.STRIP_LENGTH);
            } else {
                m138.setOffset(offset);
            }
            bestOverall = SolveMultiplex.cycle(bestOverall, m138, realM138, realMultiplexScore, 120, 1_500, saCycle);
            CtAPI.updateProgress(saCycle, saCycles + 1);
        }
        return bestOverall;
    }


    /**
     * Solve a challenge with known key
     *
     * @param offset     - if true, look for all offsets. if false, use only the real one.
     * @param cipherStr  - cipher
     * @param cribStr    - crib
     * @param realKey    - the real key
     * @param realOffset - the real offset
     */
    static void solveM138Challenge(int offset, String cipherStr, String cribStr, int maxCycles, int[] realKey, int realOffset) {
        int[] c = Utils.getText(cipherStr);
        CtAPI.printf("%s\n", Utils.getString(c));
        if (cribStr != null && cribStr.length() > 0) {
            CtAPI.printf("%s\n", cribStr);
        }
        M138 realM138 = null;
        long realMultiplexScore = -1;
        if (realKey != null) {
            realM138 = new M138(realKey, realOffset);
            realM138.setCipher(c);
            if (cribStr != null && cribStr.length() > 0) {
                realM138.setCrib(cribStr);
            }
            realMultiplexScore = realM138.eval();
            BestList.setOriginal(realM138.eval(), realM138.toString(), Utils.getString(realM138.decryption), "Original");
            CtAPI.printf("%s\n", realM138);

        }
        if (realKey != null) {
            offset = realM138.offset;
        }
        M138 m138 = new M138();
        m138.setCipher(c);
        if (cribStr != null && cribStr.length() > 0) {
            m138.setCrib(cribStr);
        }
        sa(m138, realM138, realMultiplexScore, offset, maxCycles);
    }
}
