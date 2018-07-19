package common;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;

public class BestList {

    static class BestEntry {
        long score;
        String keyString;
        String plaintextString;
        String commentString;
        BestEntry(long score, String keyString, String plaintextString, String commentString) {
            this.score = score;
            this.keyString = keyString;
            this.plaintextString = plaintextString;
            this.commentString = commentString;
        }
        void append(StringBuilder s, int rank) {
            s.append(String.format("%2d;%,12d;%s;%s;%s\n", rank, score, keyString, plaintextString, commentString));
        }
    }

    private static ArrayList<BestEntry> queue = new ArrayList<>();
    private static BestEntry original = null;
    private static int queueMaxSize = 10;
    private static long scoreThreshold = 1_800_000;

    public static void setQueueMaxSize(int queueMaxSize) {
        BestList.queueMaxSize = queueMaxSize;
        trim();
    }
    public static void clearQueue() {
        queue.clear();
    }
    public static void setOriginal(long score, String keyString, String plaintextString, String commentString) {
        original = new BestEntry(score, keyString, plaintextString, commentString);
    }

    public static synchronized boolean shouldInsert(long score) {
        if (score < scoreThreshold) {
            return false;
        }
        int size = queue.size();
        return size < queueMaxSize || score > queue.get(size - 1).score;
    }

    public static synchronized void insert(long score, String keyString, String plaintextString, String commentString) {
        for (BestEntry be : queue) {
            if (be.plaintextString.equals(plaintextString)) {
                return;
            }
        }
        int size = queue.size();
        boolean bestChanged = false;
        if (size > 0 && score > queue.get(0).score) {
            bestChanged = true;
        }
        if (size < queueMaxSize) {
            queue.add(new BestEntry(score, keyString, plaintextString, commentString));
        } else if (score > queue.get(size - 1).score) {
            BestEntry last = queue.get(size - 1);
            last.score = score;
            last.keyString = keyString;
            last.plaintextString = plaintextString;
            last.commentString = commentString;
        } else {
            return;
        }
        sort();
        display(bestChanged);
    }

    private static void trim() {
        sort();
        while (queue.size() > queueMaxSize) {
            queue.remove(queue.size() - 1);
        }
    }
    private static void sort(){
        queue.sort((o1, o2) -> (int) (o2.score - o1.score));
    }
    private static void display(boolean bestChanged){
        StringBuilder s = new StringBuilder();
        sort();
        for (int i = 0; i < queue.size(); i++) {
            queue.get(i).append(s, i + 1);
        }
        CtAPI.updateBestList(s.toString());
        if (bestChanged) {
            BestEntry first = queue.get(0);
            if (original == null) {
                CtAPI.updateScoreKeyPlaintext(first.score, first.keyString, first.plaintextString, first.commentString);
            } else {
                CtAPI.updateScoreKeyPlaintext(first.score, original.score, first.keyString, original.keyString, first.plaintextString, original.plaintextString, first.commentString, original.commentString);
            }
        }
    }
}
