package common;

import java.util.ArrayList;

public class BestResults {

    private static ArrayList<Result> bestResults = new ArrayList<>();
    private static int maxNumberOfResults = 10;
    private static Result originalResult = null;
    private static long scoreThreshold = 1_800_000;
    private static long lastBestListUpdateMillis = 0;
    private static boolean shouldUpdateBestList = false;

    public static synchronized void setMaxNumberOfResults(int maxNumberOfResults) {
        BestResults.maxNumberOfResults = maxNumberOfResults;
        clean();
    }
    public static synchronized void setScoreThreshold(long scoreThreshold) {
        BestResults.scoreThreshold = scoreThreshold;
        clean();
    }
    public static synchronized void clear() {
        bestResults.clear();
        CtAPI.displayBestList("-");
    }
    public static synchronized void setOriginal(long score, String keyString, String plaintextString, String commentString) {
        originalResult = new Result(score, keyString, plaintextString, commentString);
    }
    public static synchronized boolean shouldPushResult(long score) {
        if (shouldUpdateBestList && System.currentTimeMillis() - lastBestListUpdateMillis > 1000) {
            lastBestListUpdateMillis = System.currentTimeMillis();
            shouldUpdateBestList = false;
            display();
        }


        if (score < scoreThreshold) {
            return false;
        }
        int size = bestResults.size();
        return size < maxNumberOfResults || score > bestResults.get(size - 1).score;
    }
    public static synchronized void pushResult(long score, String keyString, String plaintextString, String commentString) {
        for (Result be : bestResults) {
            if (be.plaintextString.equals(plaintextString)) {
                return;
            }
        }
        int size = bestResults.size();
        boolean bestChanged = false;
        if (size > 0 && score > bestResults.get(0).score) {
            bestChanged = true;
        }
        if (size < maxNumberOfResults) {
            bestResults.add(new Result(score, keyString, plaintextString, commentString));
        } else if (score > bestResults.get(size - 1).score) {
            Result last = bestResults.get(size - 1);
            last.score = score;
            last.keyString = keyString;
            last.plaintextString = plaintextString;
            last.commentString = commentString;
        } else {
            return;
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
        shouldUpdateBestList = true;
        //display(bestChanged);
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
    private static synchronized void sort(){
        bestResults.sort((o1, o2) -> (int) (o2.score - o1.score));
    }
    public static synchronized void display(){
        StringBuilder s = new StringBuilder();
        sort();
        for (int i = 0; i < bestResults.size(); i++) {
            bestResults.get(i).append(s, i + 1);
        }
        CtAPI.displayBestList(s.toString());
    }
}
