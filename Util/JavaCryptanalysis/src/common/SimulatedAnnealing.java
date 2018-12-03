package common;

public class SimulatedAnnealing {
    public static boolean acceptHexaScore(long newScore, long currLocalScore, int multiplier) {

        return accept(newScore, currLocalScore, 275.0 * multiplier / 20.0);

    }

    public static boolean accept(long newScore, long currLocalScore, double temperature) {

        long diffScore = newScore - currLocalScore;
        if (diffScore > 0) {
            return true;
        }
        if (temperature == 0.0) {
            return false;
        }
        double ratio = diffScore / temperature;
        double prob = Math.pow(Math.E, ratio);
        double probThreshold = Utils.randomNextDouble();
        return prob > probThreshold && prob > 0.0085;
    }
}
