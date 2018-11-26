package common;

class Result {
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
    void append(StringBuilder s, int rank) {
        s.append(String.format("%2d;%,12d;%s;%s;%s\n", rank, score, keyStringShort, plaintextString, commentString));
    }
}
