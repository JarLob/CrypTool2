package common;

import java.util.ArrayList;

public class CommandLineArgument {

    enum Type {BOOLEAN, NUMERIC, STRING}

    Flag flag;
    String flagString;
    Type type;
    String shortDesc;
    String longDesc;

    boolean required;
    boolean multiple;

    int minIntValue;
    int maxIntValue;
    int defaultIntValue;

    String defaultStringValue = "";
    String[] validStringValues = null;
    String validStringValuesString = null;

    // Values
    boolean booleanValue = false;
    ArrayList<Integer> integerArrayList = new ArrayList<>();
    ArrayList<String> stringArrayList = new ArrayList<>();

    boolean set = false;


    public CommandLineArgument(Flag flag,
                               String flagString,
                               String shortDesc,
                               String longDesc) {
        this.flag = flag;
        this.flagString = flagString;
        this.type = Type.BOOLEAN;
        this.longDesc = longDesc;
        this.shortDesc = shortDesc;

        this.required = false;
        this.multiple = false;

    }

    public CommandLineArgument(Flag flag,
                               String flagString,
                               String shortDesc,
                               String longDesc,

                               boolean required,

                               int minIntValue,
                               int maxIntValue,
                               int defaultIntValue) {
        this.flag = flag;
        this.flagString = flagString;
        this.type = Type.NUMERIC;
        this.longDesc = longDesc;
        this.shortDesc = shortDesc;

        this.required = required;
        this.multiple = false;

        this.minIntValue = minIntValue;
        this.maxIntValue = maxIntValue;
        this.defaultIntValue = defaultIntValue;

    }

    public CommandLineArgument(Flag flag,
                               String flagString,
                               String shortDesc,
                               String longDesc,

                               boolean required,

                               int minIntValue,
                               int maxIntValue) {
        this.flag = flag;
        this.flagString = flagString;
        this.type = Type.NUMERIC;
        this.longDesc = longDesc;
        this.shortDesc = shortDesc;

        this.required = required;
        this.multiple = true;

        this.minIntValue = minIntValue;
        this.maxIntValue = maxIntValue;
    }


    public CommandLineArgument(Flag flag,
                               String flagString,
                               String shortDesc,
                               String longDesc,

                               boolean required,

                               String defaultStringValue) {
        this.flag = flag;
        this.flagString = flagString;
        this.type = Type.STRING;
        this.longDesc = longDesc;
        this.shortDesc = shortDesc;

        this.required = required;
        this.multiple = false;

        this.defaultStringValue = defaultStringValue;
        this.validStringValues = null;


    }
    public CommandLineArgument(Flag flag,
                               String flagString,
                               String shortDesc,
                               String longDesc,

                               boolean required,

                               String defaultStringValue,
                               String[] validStringValues) {
        this.flag = flag;
        this.flagString = flagString;
        this.type = Type.STRING;
        this.longDesc = longDesc;
        this.shortDesc = shortDesc;

        this.required = required;
        this.multiple = false;

        this.defaultStringValue = defaultStringValue;
        this.validStringValues = validStringValues;

        validStringValuesString = "";
        for (String validStringValue : validStringValues) {
            if (validStringValuesString.length() > 0) {
                validStringValuesString += ", ";
            }
            validStringValuesString += validStringValue;
        }
    }



}
