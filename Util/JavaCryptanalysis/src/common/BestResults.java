package common;

import java.util.ArrayList;

public class BestResults {
    static class Result {
        long score;
        String keyString;
        String keyStringShort;
        String plaintextString;
        String commentString;
        Result(long score, String keyString,String keyStringShort, String plaintextString, String commentString) {
            set(score, keyString, keyStringShort, plaintextString, commentString);
        }
        void set(long score, String keyString,String keyStringShort, String plaintextString, String commentString) {
            this.score = score;
            this.keyString = keyString;
            this.keyStringShort = keyStringShort;
            this.plaintextString = plaintextString;
            this.commentString = commentString;
        }
        public String toString(int rank) {
            return String.format("%2d;%,12d;%s;%s;%s\n", rank, score, keyStringShort, plaintextString, commentString);
        }

    }

    private static ArrayList<Result> bestResults = new ArrayList<>();
    private static Result originalResult = null;
    private static long lastBestListUpdateMillis = 0;
    private static boolean shouldUpdateBestList = false;

    private static int maxNumberOfResults = 10;
    private static long scoreThreshold = 0;
    private static boolean discardSamePlaintexts = false;
    private static boolean throttle = false;

    public static synchronized void setMaxNumberOfResults(int maxNumberOfResults) {
        BestResults.maxNumberOfResults = maxNumberOfResults;
        clean();
    }
    public static synchronized void setScoreThreshold(long scoreThreshold) {
        BestResults.scoreThreshold = scoreThreshold;
        clean();
    }
    public static synchronized void setDiscardSamePlaintexts(boolean discardSamePlaintexts) {
        BestResults.discardSamePlaintexts = discardSamePlaintexts;
        clean();
    }
    public static synchronized void setThrottle(boolean throttle) {
        BestResults.throttle = throttle;
        clean();
    }
    public static synchronized void clear() {
        bestResults.clear();
        CtAPI.displayBestList("-");
    }
    public static synchronized void setOriginal(long score, String keyString, String keyStringShort,String plaintextString, String commentString) {
        originalResult = new Result(score, keyString, keyStringShort, plaintextString, commentString);
    }
    public static synchronized boolean shouldPushResult(long score) {

        if (throttle) {
            if (shouldUpdateBestList && System.currentTimeMillis() - lastBestListUpdateMillis > 1000) {
                lastBestListUpdateMillis = System.currentTimeMillis();
                shouldUpdateBestList = false;
                display();
            }
        }

        if (score < scoreThreshold) {
            return false;
        }
        int size = bestResults.size();
        return size < maxNumberOfResults || score > bestResults.get(size - 1).score;
    }
    public static synchronized boolean pushResult(long score, String keyString, String keyStringShort, String plaintextString, String commentString) {
        if (discardSamePlaintexts) {
            for (Result be : bestResults) {
                if (be.plaintextString.equals(plaintextString)) {
                    return false;
                }
            }
        }
        for (Result be : bestResults) {
            if (be.keyString.equals(keyString)) {
                return false;
            }
        }
        int size = bestResults.size();
        boolean bestChanged = false;
        if (size == 0 || score > bestResults.get(0).score) {
            bestChanged = true;
        }
        if (size < maxNumberOfResults) {
            bestResults.add(new Result(score, keyString, keyStringShort, plaintextString, commentString));
        } else if (score > bestResults.get(size - 1).score) {
            bestResults.get(size - 1).set(score, keyString, keyStringShort, plaintextString, commentString);
        } else {
            return false;
        }
        sort();
        if (bestChanged) {
            Result bestResult = bestResults.get(0);
            if (originalResult == null) {
                CtAPI.displayBestResult(bestResult);
            } else {
                CtAPI.displayBestResult(bestResult, originalResult);
            }
        }
        if (throttle) {
            shouldUpdateBestList = true;
        } else {
            display();
        }
        return true;
    }
    public static synchronized void display() {
        StringBuilder s = new StringBuilder();
        sort();
        for (int i = 0; i < bestResults.size(); i++) {
            s.append(bestResults.get(i).toString(i + 1));
        }
        CtAPI.displayBestList(s.toString());
    }

    private static synchronized void clean() {
        sort();
        while (bestResults.size() > maxNumberOfResults) {
            bestResults.remove(bestResults.size() - 1);
        }
        while (!bestResults.isEmpty() && bestResults.get(bestResults.size() - 1).score < scoreThreshold) {
            bestResults.remove(bestResults.size() - 1);
        }
    }
    private static synchronized void sort() {
        bestResults.sort((o1, o2) -> (int) (o2.score - o1.score));
    }
}
