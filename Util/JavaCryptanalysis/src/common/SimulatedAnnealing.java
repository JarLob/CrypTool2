package common;

public class SimulatedAnnealing {
    public static boolean acceptHexaScore(long newScore, long currLocalScore, int multiplier) {

        long diffScore = newScore - currLocalScore;
        if (diffScore > 0) {
            return true;
        }

        double temperature = 275.0 * multiplier / 20.0;
        double ratio = diffScore / temperature;
        double prob = Math.pow(Math.E, ratio);
        double probThreshold = Utils.randomNextDouble();
        return prob > probThreshold && prob > 0.0085;

    }
}
