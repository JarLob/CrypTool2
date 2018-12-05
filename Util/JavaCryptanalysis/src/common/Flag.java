package common;

public enum Flag {
    RESOURCE_PATH("r"),
    CIPHERTEXT("i"),
    CRIB("p"),
    SIMULATION_TEXT_LENGTH("l"),
    THREADS("t"),
    OFFSET("o"),
    CYCLES("n"),
    MODEL("m"),
    SIMULATION("s"),
    VERSION("y"),
    LANGUAGE("g"),
    SIMULATION_OVERLAPS("o"),
    INDICATORS_FILE("d"),
    KEY("k"),
    MODE("o"),
    CRIB_POSITION("j"),
    VERBOSE("u"),
    MESSAGE_INDICATOR("w"),
    RIGHT_ROTOR_SAMPLING("x"),
    MIDDLE_RING_SCOPE("y"),
    HC_SA_CYCLES("h"),
    SCENARIO("z"),
    SCENARIO_PATH("f");
    String string;
    private Flag(String string) {
        this.string = string;
    }
    @Override
    public String toString() {
        return string;
    }
}
