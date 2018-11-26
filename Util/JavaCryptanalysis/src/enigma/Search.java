package enigma;


import common.BestResults;
import common.CtAPI;

import static java.lang.Thread.sleep;

class Search {

    public static void searchTrigramIC(Key from, Key to, boolean findSettingsIc,
                                       MRingScope lRingSettingScope, int rRingSpacing,
                                       boolean hcEveryBest, int hcMaxPass, int minTrigramsScoreToPrint, int THREADS, byte[] ciphertext, int len,
                                       String indicatorS, String indicatorMessageKeyS) {

        Key ckey = new Key(from);
        Key lo = new Key(from);
        Key bestKey = null;

        Key high = new Key(to);

        double best = 0.0;

        byte plaintext[] = new byte[Key.MAXLEN];

        long counter = 0;

        boolean checkForIndicatorMatch = false;
        if (indicatorS.length() * indicatorMessageKeyS.length() != 0)
            checkForIndicatorMatch = true;

        if (lo.mRing == high.mRing)
            lRingSettingScope = MRingScope.ALL;
        long totalKeys = Key.numberOfPossibleKeys(lo, high, len, lRingSettingScope, rRingSpacing, checkForIndicatorMatch);

        long normalizedNkeys = (totalKeys * len) / 250;

        int minRate;
        int maxRate;

        if (lRingSettingScope == MRingScope.ALL) {
            minRate = 100000;
            maxRate = 150000;
        } else if (lRingSettingScope == MRingScope.ONE_NON_STEPPING) {
            minRate = 20000;
            maxRate = 30000;
        } else {
            minRate = 50000;
            maxRate = 75000;
        }

        CtAPI.printf("\n\nSTARTING %s SEARCH: Number of Keys to search: %d \n\n", findSettingsIc ? "IC" : "TRIGRAM", totalKeys);
        CtAPI.printf("Estimated Search Time: %s\n\n", Utils.getEstimatedTimeString(normalizedNkeys, minRate, maxRate));

        long startTime = System.currentTimeMillis();

        for (ckey.ukwNum = lo.ukwNum; ckey.ukwNum <= high.ukwNum; ckey.ukwNum++) {
            for (ckey.gSlot = lo.gSlot; ckey.gSlot <= high.gSlot; ckey.gSlot++) {
                for (ckey.lSlot = lo.lSlot; ckey.lSlot <= high.lSlot; ckey.lSlot++) {
                    for (ckey.mSlot = lo.mSlot; ckey.mSlot <= high.mSlot; ckey.mSlot++) {
                        if (ckey.mSlot == ckey.lSlot) continue;
                        for (ckey.rSlot = lo.rSlot; ckey.rSlot <= high.rSlot; ckey.rSlot++) {
                            if (ckey.rSlot == ckey.lSlot || ckey.rSlot == ckey.mSlot) continue;
                            for (ckey.gRing = lo.gRing; ckey.gRing <= high.gRing; ckey.gRing++) {
                                for (ckey.lRing = lo.lRing; ckey.lRing <= high.lRing; ckey.lRing++) {
                                    for (ckey.mRing = lo.mRing; ckey.mRing <= high.mRing; ckey.mRing++) {
                                        for (ckey.rRing = lo.rRing; ckey.rRing <= high.rRing; ckey.rRing++) {
                                            if ((ckey.rRing % rRingSpacing) != 0)
                                                continue;
                                            Key keyFromIndicator = null;
                                            if (checkForIndicatorMatch)
                                                keyFromIndicator = ckey.getKeyFromIndicator(indicatorS, indicatorMessageKeyS);
                                            for (ckey.gMesg = lo.gMesg; ckey.gMesg <= high.gMesg; ckey.gMesg++) {
                                                for (ckey.lMesg = lo.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++) {
                                                    if ((checkForIndicatorMatch) && (ckey.lMesg != keyFromIndicator.lMesg))
                                                        continue;
                                                    for (ckey.mMesg = lo.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++) {
                                                        if ((checkForIndicatorMatch) && (ckey.mMesg != keyFromIndicator.mMesg))
                                                            continue;
                                                        for (ckey.rMesg = lo.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++) {
                                                            if ((checkForIndicatorMatch) && (ckey.rMesg != keyFromIndicator.rMesg))
                                                                continue;

                                                            if (lRingSettingScope != MRingScope.ALL) {
                                                                int mRingSteppingPos = ckey.getLeftRotorSteppingPosition(len);
                                                                if (!Key.CheckValidWheelsState(len, mRingSteppingPos, lRingSettingScope))
                                                                    continue;
                                                            }

                                                            counter++;
                                                            ReportResult.displayProgress(counter, totalKeys);

                                                            if (findSettingsIc)
                                                                ckey.score = (int) (100000.0 * ckey.icScoreWithoutLookupBuild(ciphertext, len));
                                                            else
                                                                ckey.score = ckey.triScoreWithoutLookupBuild(ciphertext, len);

                                                            if (ckey.score < minTrigramsScoreToPrint) {
                                                                continue;
                                                            }
                                                            if (ckey.score - best >= 0) {
                                                                best = ckey.score;
                                                                bestKey = new Key(ckey);

                                                                if ((hcEveryBest) && (hcMaxPass > 0)) {
                                                                    if ((findSettingsIc && (ckey.score > 3500)) || (!findSettingsIc && (ckey.score > 10000))) {
                                                                        HillClimb.hillClimbRange(bestKey, bestKey, hcMaxPass, THREADS,
                                                                                minTrigramsScoreToPrint, MRingScope.ALL, 1, ciphertext, len);

                                                                    }
                                                                }
                                                            }


                                                            if (BestResults.shouldPushResult(ckey.score)) {
                                                                ckey.encipherDecipherAll(ciphertext, plaintext, len);
                                                                String plains = Utils.getString(plaintext, len);

                                                                long elapsed = System.currentTimeMillis() - startTime;
                                                                String desc = String.format("%s [%,5dK/%,5dK][%,4dK/sec][%,4d Sec]",
                                                                        findSettingsIc ? "IC" : "TRIGRAMS", counter / 1000, totalKeys / 1000, counter / (elapsed + 1), elapsed / 1000);

                                                                ReportResult.reportResult(0, ckey, ckey.score, plains, desc);

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        CtAPI.printf("\n\nSEARCH ENDED: Total %d in %.1f Seconds (%d/Sec)\n\n\n",
                counter,
                (System.currentTimeMillis() - startTime) / 1000.0,
                1000 * counter / (System.currentTimeMillis() - startTime + 1));


        if ((bestKey != null) && (hcMaxPass > 0))
            HillClimb.hillClimbRange(bestKey, bestKey, hcMaxPass, THREADS,
                    findSettingsIc ? 0 : minTrigramsScoreToPrint, MRingScope.ALL, 1, ciphertext, len);

    }

    private static int scoreForMenus(double tri, double ic) {

        int res;
        if (tri > 10000.0)
            res = (int) tri;
        else if (ic > 0.050)
            res = (int) (10000.0 * ic / 0.050);
        else
            res = (int) (tri * ic / 0.050);
        if (res < 3000)
            return 0;
        else
            return res;
    }

    public static Key searchZSheets(byte[] indicData, int flen, Key from, Key to, byte[] ciphertext, int clen) {


        int nIndics = flen / 9;  // initial value for buffer allocation

        //nIndics = 10;

        boolean[][] females = new boolean[nIndics][3];
        byte[][] indicCiphertextWithFemales = new byte[nIndics][6];
        byte[][] indicMsgKeysWithFemales = new byte[nIndics][3];
        int indicsWithFemales = 0;

        byte[][] indicCiphertext = new byte[nIndics][6];
        byte[][] indicMsgKeys = new byte[nIndics][3];

        for (int i = 0; i < nIndics; i++) {
            boolean hasFemales[] = new boolean[3];
            for (int pos = 0; pos < 3; pos++) {
                int letterAtPos = indicData[i * 9 + 3 + pos];
                int letterAtPosPlus3 = indicData[i * 9 + 3 + pos + 3];
                if (letterAtPos == letterAtPosPlus3)
                    hasFemales[pos] = true;


            }
            if (hasFemales[0] || hasFemales[1] || hasFemales[2]) {
                System.arraycopy(indicData, i * 9, indicMsgKeysWithFemales[indicsWithFemales], 0, 3);
                System.arraycopy(indicData, i * 9 + 3, indicCiphertextWithFemales[indicsWithFemales], 0, 6);
                System.arraycopy(hasFemales, 0, females[indicsWithFemales], 0, 3);

                indicsWithFemales++;

            }
            System.arraycopy(indicData, i * 9, indicMsgKeys[i], 0, 3);
            System.arraycopy(indicData, i * 9 + 3, indicCiphertext[i], 0, 6);
        }

        int nRingsToTest = (to.lRing - from.lRing + 1) * (to.mRing - from.mRing + 1) * (to.rRing - from.rRing + 1);
        if ((indicsWithFemales < 5) || ((indicsWithFemales < 10) && (nRingsToTest > 10))) {
            CtAPI.printf("\n\nINDICATORS1938 Zygalski Sheets Search: Only %d indicators with 'females' (out of %d) were found - not enough (best 10 or more) or will take too much time.... \n\n", indicsWithFemales, nIndics);
        }
        CtAPI.printf("\n\nINDICATORS1938 Zygalski Sheets Search: %d indicators with 'females' (out of %d) - Starting search.... \n\n", indicsWithFemales, nIndics);


        Key ckey = new Key(from);
        Key lo = new Key(from);
        Key high = new Key(to);
        lo.lMesg = high.lMesg = lo.mMesg = high.mMesg = lo.rMesg = high.rMesg = 0;

        long counterKeys = 0;
        long totalKeys = Key.numberOfPossibleKeys(lo, high, 6, MRingScope.ALL, 1, false);

        long startTime = System.currentTimeMillis();

        for (ckey.ukwNum = lo.ukwNum; ckey.ukwNum <= high.ukwNum; ckey.ukwNum++) {
            for (ckey.gSlot = lo.gSlot; ckey.gSlot <= high.gSlot; ckey.gSlot++) {
                for (ckey.lSlot = lo.lSlot; ckey.lSlot <= high.lSlot; ckey.lSlot++) {
                    for (ckey.mSlot = lo.mSlot; ckey.mSlot <= high.mSlot; ckey.mSlot++) {
                        if (ckey.mSlot == ckey.lSlot) continue;
                        for (ckey.rSlot = lo.rSlot; ckey.rSlot <= high.rSlot; ckey.rSlot++) {
                            if (ckey.rSlot == ckey.lSlot || ckey.rSlot == ckey.mSlot) continue;
                            for (ckey.gRing = lo.gRing; ckey.gRing <= high.gRing; ckey.gRing++) {
                                for (ckey.lRing = lo.lRing; ckey.lRing <= high.lRing; ckey.lRing++) {
//                                    if (indicsWithFemales > 10) {
//                                        ckey.mRing = ckey.lRing = 0;
//                                        ckey.score = 0;
//                                        ckey.setStecker("");
//                                        ckey.printKeyString("Candidate");
//                                    }
                                    for (ckey.mRing = lo.mRing; ckey.mRing <= high.mRing; ckey.mRing++) {
                                        for (ckey.rRing = lo.rRing; ckey.rRing <= high.rRing; ckey.rRing++) {
                                            counterKeys++;
                                            ReportResult.displayProgress(counterKeys, totalKeys);
                                            boolean valid = true;
                                            for (int indic = 0; indic < indicsWithFemales; indic++) {

                                                ckey.lMesg = indicMsgKeysWithFemales[indic][0];
                                                ckey.mMesg = indicMsgKeysWithFemales[indic][1];
                                                ckey.rMesg = indicMsgKeysWithFemales[indic][2];

                                                if (!ckey.checkFemales(females[indic], false)) {
                                                    valid = false;
                                                    break;
                                                }

                                            }
                                            if (valid) {



                                                // clean up stecker info from previous runs.
                                                ckey.setStecker("");
                                                int runs;
                                                for (runs = 0; runs < 5; runs++) {
                                                    if (runs > 0) {
                                                        ckey.setRandomStb(6);
                                                    }
                                                    int prevScore = ckey.score = 0;
                                                    while (true) {
                                                        HillClimb.hillClimbIndicator(ckey, indicMsgKeys, indicCiphertext, nIndics, false);
                                                        if ((ckey.score <= prevScore) || (ckey.score == 1000)) {
                                                            break;
                                                        }
                                                        prevScore = ckey.score;
                                                    }
                                                    if (ckey.score == 1000) {
                                                        break;
                                                    }
                                                }

                                                if (ckey.score > 350) {
                                                    String all = indicsString(indicCiphertext, indicMsgKeys, nIndics, ckey);

                                                    if (ckey.score > 700) {
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Found Wheels Order (%d%d%d) and Ring Settings (%s) which matches the female cases (%d keys tested)\n\n",
                                                                ckey.lSlot, ckey.mSlot, ckey.rSlot,
                                                                "" + Utils.getChar(ckey.lRing) + Utils.getChar(ckey.mRing) + Utils.getChar(ckey.rRing),
                                                                counterKeys);
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Found Stecker Board settings matching (score %d) the double-encrypted indicators: %s\n\n", ckey.score, ckey.stbString());

                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Showing only deciphered indicators with females (%d):\n\n", indicsWithFemales);
                                                        String withFemales = indicsString(indicCiphertextWithFemales, indicMsgKeysWithFemales, indicsWithFemales, ckey);
                                                        CtAPI.printf(withFemales);
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Showing all deciphered indicators (%d):\n\n", nIndics);
                                                        CtAPI.printf(all);
                                                    }
                                                    ckey.lMesg = ckey.mMesg = ckey.rMesg = 0;
                                                    long elapsed = System.currentTimeMillis() - startTime;
                                                    String desc = String.format("INDICATORS1938 DAILY KEY [%,5dK/%,5dK][%,4dK/sec][%,4d Sec]",
                                                            counterKeys / 1000, totalKeys / 1000, counterKeys / elapsed, elapsed / 1000);
                                                    ReportResult.reportResult(0, ckey, ckey.score, all.replaceAll("\n", ""), desc);

                                                    // Use the first indicator (plain) as message key to decipher the actual message key.
                                                    if (ckey.score >= 900 && clen > 0) {
                                                        byte[] indicPlaintext = new byte[6];
                                                        Key indicKey = new Key(ckey);
                                                        indicKey.lMesg = indicData[0];
                                                        indicKey.mMesg = indicData[1];
                                                        indicKey.rMesg = indicData[2];
                                                        indicKey.score = 0;
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Using the non-encrypted indicator (%s) of the first indicator set (%s) in the the original file to decipher the (double encrypted) message key.\n\n",
                                                                "" + Utils.getChar(indicKey.lMesg) + Utils.getChar(indicKey.mMesg) + Utils.getChar(indicKey.rMesg),
                                                                Utils.getString(indicData, 9));
                                                        indicKey.printKeyString("");
                                                        byte doubleKeyText[] = new byte[6];
                                                        System.arraycopy(indicData, 3, doubleKeyText, 0, 6);
                                                        indicKey.encipherDecipherAll(doubleKeyText, indicPlaintext, 6);
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Message Key deciphered  - Making sure it is doubled .... (%s) \n",
                                                                Utils.getString(indicPlaintext, 6));

                                                        if ((indicPlaintext[0] != indicPlaintext[3]) ||
                                                                (indicPlaintext[1] != indicPlaintext[4]) ||
                                                                (indicPlaintext[2] != indicPlaintext[5])) {
                                                            CtAPI.print("\n\nINDICATORS1938 Zygalski Sheets Search: Problem - the first indicator is not doubled \n");
                                                            continue;
                                                        }

                                                        ckey.lMesg = indicPlaintext[0];
                                                        ckey.mMesg = indicPlaintext[1];
                                                        ckey.rMesg = indicPlaintext[2];
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Obtained the message key for the message (%s after decryption) - Updating the key.\n\n",
                                                                "" + Utils.getChar(ckey.lMesg) + Utils.getChar(ckey.mMesg) + Utils.getChar(ckey.rMesg));

                                                        ckey.printKeyString("INDICATORS1938 Zygalski Sheets Search: Key to decipher the message");

                                                        byte[] plaintext = new byte[Key.MAXLEN];

                                                        ckey.encipherDecipherAll(ciphertext, plaintext, clen);
                                                        String plainS = Utils.getCiphertextStringNoXJ(plaintext, clen);
                                                        CtAPI.printf("\nWithout X/J\n\n%s\n", plainS);

                                                        plainS = Utils.getString(plaintext, clen);

                                                        CtAPI.printf("Z. Sheets Search Successful - Plaintext is: \n\n%s\n", plainS);
                                                        byte[] steppings = new byte[Key.MAXLEN];
                                                        ckey.showSteppings(steppings, clen);
                                                        String steppingsS = Utils.getCiphertextStringNoXJ(steppings, clen);
                                                        System.out.println(steppingsS);
                                                        ckey.score = ckey.triScoreWithoutLookupBuild(ciphertext, clen);
                                                        ReportResult.reportResult(0, ckey, ckey.score,
                                                                plainS + "     Indicators:" + all.replaceAll("\n", ""),
                                                                desc.replaceAll("INDICATORS1938 DAILY KEY", "TRIGRAMS"));

                                                       return ckey;
                                                    }
                                                }
                                            } // if valid

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        long elapsed = (System.currentTimeMillis() - startTime + 1);


        CtAPI.printf("INDICATORS1938 Zygalski Sheets Search: No matching keys found \n %d Total Keys Tested in %.1f Seconds(%d/sec)\n\n",
                counterKeys, elapsed / 1000.0, 1000 * counterKeys / elapsed);

        return null;


    }

    private static String indicsString(byte[][] indicCiphertext, byte[][] indicMsgKeys, int indicsWith, Key ckey) {
        String s = "";
        Key indicKey = new Key(ckey);
        for (int i = 0; i < indicsWith; i++) {
            byte[] indicPlaintext = new byte[6];

            indicKey.lMesg = indicMsgKeys[i][0];
            indicKey.mMesg = indicMsgKeys[i][1];
            indicKey.rMesg = indicMsgKeys[i][2];
            indicKey.encipherDecipherAll(indicCiphertext[i], indicPlaintext, 6);

            s += String.format("%s:%s ==> %s, ",
                    Utils.getString(indicMsgKeys[i], 3),
                    Utils.getString(indicCiphertext[i], 6),
                    Utils.getString(indicPlaintext, 6));
            if ((i % 6) == 5)
                s += "\n";
        }
        s += "\n";
        return s;
    }

    public static Key searchCycleMatch(byte[] indicData, int flen, Key from, Key to, int HILLCLIMBING_CYCLES, int THREADS, byte[] ciphertext, int clen,
                                       MRingScope lRingSettingScope, int rRingSpacing) {


        int nIndics = flen / 6;

        //nIndics -= 4;

        int[][] dbCycleSizes = new int[3][26];
        int[][] links = new int[3][26];
        for (int pos = 0; pos < 3; pos++)
            for (int l = 0; l < 26; l++)
                links[pos][l] = -1;

        for (int i = 0; i < nIndics; i++) {
            for (int pos = 0; pos < 3; pos++) {
                int letterAtPos = indicData[i * 6 + pos];
                int letterAtPosPlus3 = indicData[i * 6 + pos + 3];
                if ((links[pos][letterAtPos] != -1) && (links[pos][letterAtPos] != letterAtPosPlus3)) {
                    CtAPI.printf("Cycle Match Search: Conflict with Indicator #%d with letter [%s] (position %d) and letter [%s] (position %d)\n",
                            i, Utils.getChar(letterAtPos), pos, Utils.getChar(letterAtPosPlus3), pos + 3);
                } else {
                    links[pos][letterAtPos] = letterAtPosPlus3;
                }

            }
        }
        Key ckey = new Key(from);
        Key lo = new Key(from);
        Key high = new Key(to);

        long counterKeys = 0;

        if (lo.mRing == high.mRing)
            lRingSettingScope = MRingScope.ALL;
        long totalKeys = Key.numberOfPossibleKeys(lo, high, 6, lRingSettingScope, rRingSpacing, false);


        boolean hillClimbOnly = false;
        if (!Key.buildCycles(links, dbCycleSizes, true)) {
            CtAPI.print("Cycle Match Search: Not enough indicators/cycles are not complete.\n");
            CtAPI.print("Cycle Match Search: Cycles are ignored - searching only via Hill Climbing.\n");
            hillClimbOnly = true;
        }

        if (lo.mRing == high.mRing)
            lRingSettingScope = MRingScope.ALL;

        CtAPI.print("\nCycle Match Search: Starting ....\n\n");
        int matchingKeys = 0;
        long startTime = System.currentTimeMillis();

        for (ckey.ukwNum = lo.ukwNum; ckey.ukwNum <= high.ukwNum; ckey.ukwNum++) {
            for (ckey.gSlot = lo.gSlot; ckey.gSlot <= lo.gSlot; ckey.gSlot++) {
                for (ckey.lSlot = lo.lSlot; ckey.lSlot <= high.lSlot; ckey.lSlot++) {
                    for (ckey.mSlot = lo.mSlot; ckey.mSlot <= high.mSlot; ckey.mSlot++) {
                        if (ckey.mSlot == ckey.lSlot) continue;
                        for (ckey.rSlot = lo.rSlot; ckey.rSlot <= high.rSlot; ckey.rSlot++) {
                            if (ckey.rSlot == ckey.lSlot || ckey.rSlot == ckey.mSlot) continue;

                            ckey.setStecker("");
                            ckey.lMesg = ckey.mMesg = ckey.rMesg = -1;
                            ckey.lRing = ckey.mRing = ckey.rRing = -1;
                            ckey.score = 0;
                            ckey.printKeyString("Progress....");

                            for (ckey.gRing = lo.gRing; ckey.gRing <= lo.gRing; ckey.gRing++) {
                                for (ckey.lRing = lo.lRing; ckey.lRing <= high.lRing; ckey.lRing++) {
                                    for (ckey.mRing = lo.mRing; ckey.mRing <= high.mRing; ckey.mRing++) {
                                        for (ckey.rRing = lo.rRing; ckey.rRing <= high.rRing; ckey.rRing++) {
                                            if ((ckey.rRing % rRingSpacing) != 0)
                                                continue;
                                            for (ckey.gMesg = lo.gMesg; ckey.gMesg <= lo.gMesg; ckey.gMesg++) {
                                                for (ckey.lMesg = lo.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++) {
                                                    if (hillClimbOnly) {
                                                        ckey.score = 0;
                                                        ckey.setStecker("");
                                                        ckey.printKeyString("Progress....");
                                                    }
                                                    for (ckey.mMesg = lo.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++) {
                                                        for (ckey.rMesg = lo.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++) {


                                                            if (lRingSettingScope != MRingScope.ALL) {
                                                                int mRingSteppingPos = ckey.getLeftRotorSteppingPosition(6);
                                                                if (!Key.CheckValidWheelsState(6, mRingSteppingPos, lRingSettingScope))
                                                                    continue;
                                                            }
                                                            counterKeys++;
                                                            ReportResult.displayProgress(counterKeys, totalKeys);
                                                            boolean valid = false;

                                                            if (!hillClimbOnly)
                                                                valid = ckey.buildAndCompareCycles(dbCycleSizes, true);

                                                            if (valid || hillClimbOnly) {

                                                                if (!hillClimbOnly) {
                                                                    ckey.score = 0;
                                                                    ckey.setStecker("");
                                                                    //ckey.printKeyString("Candidate");
                                                                }
                                                                byte[][] indicCiphertext = new byte[nIndics][6];
                                                                for (int i = 0; i < nIndics; i++)
                                                                    System.arraycopy(indicData, i * 6, indicCiphertext[i], 0, 6);

                                                                // clean up stecker info from previous runs.
                                                                ckey.setStecker("");
                                                                int runs;
                                                                for (runs = 0; runs < (hillClimbOnly ? (nIndics <= 10 ? 5 : 2) : 100); runs++) {
                                                                    if (runs > 0) {
                                                                        ckey.setRandomStb(4);
                                                                    }
                                                                    int prevScore = ckey.score = 0;
                                                                    while (true) {
                                                                        HillClimb.hillClimbIndicator(ckey, null, indicCiphertext, nIndics, false);

                                                                        if ((ckey.score <= prevScore) || (ckey.score == 1000)) {
                                                                            break;
                                                                        }
                                                                        prevScore = ckey.score;
                                                                    }
                                                                    if (ckey.score == 1000) {
                                                                        break;
                                                                    }
                                                                }

                                                                if (ckey.score > 660) {
                                                                    ckey.printKeyString("Candidate");

                                                                    long elapsed = System.currentTimeMillis() - startTime;
                                                                    String desc = String.format("INDICATORS DAILY KEY [%,5dK/%,5dK][%,4dK/sec][%,4d Sec]",
                                                                            counterKeys / 1000, totalKeys / 1000, counterKeys / elapsed, elapsed / 1000);
                                                                    byte[] indicPlaintext = new byte[6];

                                                                    String allIndicsPlaintext = "";
                                                                    for (int i = 0; i < nIndics; i++) {
                                                                        ckey.encipherDecipherAll(indicCiphertext[i], indicPlaintext, 6);
                                                                        allIndicsPlaintext += Utils.getString(indicPlaintext, 6) + "   ";
                                                                    }
                                                                    if (BestResults.shouldPushResult(ckey.score)) {
                                                                        ReportResult.reportResult(0, ckey, ckey.score, allIndicsPlaintext, desc);
                                                                    }

                                                                    CtAPI.printf("\nINDICATORS Cycle Match Search: Found Message Key settings (%s) which matches the cycle patterns of the indicators (%d keys tested)\n\n",
                                                                            "" + Utils.getChar(ckey.lMesg) + Utils.getChar(ckey.mMesg) + Utils.getChar(ckey.rMesg),
                                                                            counterKeys);
                                                                    CtAPI.printf("INDICATORS Cycle Match Search: Found Stecker Board settings - Score %d/1000 (%d runs) - Stecker Settings: %s (%d plugs)\n\n",
                                                                            ckey.score, runs + 1, ckey.stbString(), ckey.stbString().length() / 2);
                                                                    System.out.print("INDICATORS Cycle Match Search: Showing the deciphered double indicators:\n\n");

                                                                    String allIndicsWithNewlines = "";
                                                                    for (int i = 0; i < nIndics; i++) {
                                                                        ckey.encipherDecipherAll(indicCiphertext[i], indicPlaintext, 6);
                                                                        allIndicsWithNewlines += String.format("  %s ==> %s",
                                                                                Utils.getString(indicCiphertext[i], 6),
                                                                                Utils.getString(indicPlaintext, 6));
                                                                        if ((i % 6) == 5) {
                                                                            allIndicsWithNewlines += "\n";
                                                                        }
                                                                    }
                                                                    System.out.printf("%s\n", allIndicsWithNewlines);

                                                                    if (ckey.score > 900) {

                                                                        if (clen > 0) {
                                                                            // use the first deciphered indicator as message key to return in the key.
                                                                            ckey.encipherDecipherAll(indicCiphertext[0], indicPlaintext, 6);
                                                                            ckey.lMesg = indicPlaintext[0];
                                                                            ckey.mMesg = indicPlaintext[1];
                                                                            ckey.rMesg = indicPlaintext[2];
                                                                            CtAPI.printf("\nINDICATORS Cycle Match Search: Using the first indicator (%s after decryption) as the Message Key (still need to find out the Ring Settings - not yet correct) \n\n",
                                                                                    "" + Utils.getChar(indicPlaintext[0]) + Utils.getChar(indicPlaintext[1]) + Utils.getChar(indicPlaintext[2]));
                                                                            ckey.score = 0;
                                                                            ckey.printKeyString("");

                                                                            // only sweep ring values (with no limitation
                                                                            Key lowKey = new Key(ckey);
                                                                            lowKey.lRing = lowKey.mRing = lowKey.rRing = Utils.getIndex('A');
                                                                            Key highKey = new Key(ckey);
                                                                            highKey.lRing = highKey.mRing = highKey.rRing = Utils.getIndex('Z');
                                                                            CtAPI.print("\nINDICATORS Cycle Match Search: Running a Trigram Search to find the exact Ring Settings and decipher the message.\n");
                                                                            Search.searchTrigramIC(lowKey, highKey, false, MRingScope.ALL, 1, false, HILLCLIMBING_CYCLES, 10000, THREADS, ciphertext, clen, "", "");
                                                                            ReportResult.displayProgress(counterKeys, totalKeys);

                                                                            try {
                                                                                sleep(2000);
                                                                            } catch (InterruptedException e) {
                                                                                e.printStackTrace();
                                                                            }
                                                                            ReportResult.displayProgress(counterKeys, totalKeys);

                                                                            //return ckey;

                                                                        }
                                                                        matchingKeys++;
                                                                    }
                                                                }
                                                            } // if valid
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        long elapsed = (System.currentTimeMillis() - startTime + 1);

        CtAPI.printf("INDICATORS End of Cycle Search - %d matching keys found \n %d Total Keys Tested in %.1f Seconds(%d/sec)\n\n",
                matchingKeys, counterKeys, elapsed / 1000.0, 1000 * counterKeys / elapsed);

        return null;

    }

    public static void searchCribMenus(BombeMenu[] bombeMenus, int nMenus, Key from, Key to,
                                       MRingScope lRingSettingScope, int rRingSpacing,
                                       int hcMaxPass, int THREADS, byte[] ciphertext, int len, boolean debugMenus,
                                       String indicatorS, String indicatorMessageKeyS) {

        Key ckey = new Key(from);
        Key lo = new Key(from);
        Key high = new Key(to);

        double ic;
        int tri;

        byte plaintext[] = new byte[Key.MAXLEN];
        int nStops = 0;
        int bestscore = 0;
        int bestMenu = 0;

        final int MAXTOPS = 1000;
        Key topKeys[] = new Key[MAXTOPS];
        int nTops = 0;

        byte assumedSteckers[] = new byte[26];
        byte strength[] = new byte[26];

        boolean checkForIndicatorMatch = false;
        if (indicatorS.length() * indicatorMessageKeyS.length() != 0)
            checkForIndicatorMatch = true;

        long counter = 0;
        long counterSameMax = 0;
        long countKeys = 0;

        if (lo.mRing == high.mRing)
            lRingSettingScope = MRingScope.ALL;
        long totalKeys = Key.numberOfPossibleKeys(lo, high, len, lRingSettingScope, rRingSpacing, checkForIndicatorMatch);
        CtAPI.printf("\n\nStart Bombe search: Number of menus: %d, Number of keys: %d, Total to Check: %d\n\n", nMenus, totalKeys, nMenus * totalKeys);

        printEstimatedTimeBombeRun(totalKeys * bombeMenus[0].cribLen / 25, nMenus, lRingSettingScope);

        long start = System.currentTimeMillis();

        for (ckey.ukwNum = lo.ukwNum; ckey.ukwNum <= high.ukwNum; ckey.ukwNum++) {
            for (ckey.gSlot = lo.gSlot; ckey.gSlot <= high.gSlot; ckey.gSlot++) {
                for (ckey.lSlot = lo.lSlot; ckey.lSlot <= high.lSlot; ckey.lSlot++) {
                    for (ckey.mSlot = lo.mSlot; ckey.mSlot <= high.mSlot; ckey.mSlot++) {
                        if (ckey.mSlot == ckey.lSlot) continue;
                        for (ckey.rSlot = lo.rSlot; ckey.rSlot <= high.rSlot; ckey.rSlot++) {
                            if (ckey.rSlot == ckey.lSlot || ckey.rSlot == ckey.mSlot) continue;
                            for (ckey.gRing = lo.gRing; ckey.gRing <= high.gRing; ckey.gRing++) {
                                for (ckey.lRing = lo.lRing; ckey.lRing <= high.lRing; ckey.lRing++) {
                                    for (ckey.mRing = lo.mRing; ckey.mRing <= high.mRing; ckey.mRing++) {
                                        for (ckey.rRing = lo.rRing; ckey.rRing <= high.rRing; ckey.rRing++) {
                                            if ((ckey.rRing % rRingSpacing) != 0) continue;
                                            for (ckey.gMesg = lo.gMesg; ckey.gMesg <= high.gMesg; ckey.gMesg++) {
                                                Key keyFromIndicator = null;
                                                if (checkForIndicatorMatch)
                                                    keyFromIndicator = ckey.getKeyFromIndicator(indicatorS, indicatorMessageKeyS);
                                                for (ckey.lMesg = lo.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++) {
                                                    if ((checkForIndicatorMatch) && (ckey.lMesg != keyFromIndicator.lMesg))
                                                        continue;
                                                    for (ckey.mMesg = lo.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++) {
                                                        if ((checkForIndicatorMatch) && (ckey.mMesg != keyFromIndicator.mMesg))
                                                            continue;
                                                        for (ckey.rMesg = lo.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++) {
                                                            if ((checkForIndicatorMatch) && (ckey.lMesg != keyFromIndicator.lMesg))
                                                                continue;


                                                            if (lRingSettingScope != MRingScope.ALL) {
                                                                int lRingSteppingPos = ckey.getLeftRotorSteppingPosition(len);
                                                                if (!Key.CheckValidWheelsState(len, lRingSteppingPos, lRingSettingScope))
                                                                    continue;
                                                            }

                                                            countKeys++;
                                                            ReportResult.displayProgress(countKeys, totalKeys);
                                                            boolean foundForThisKey = false;
                                                            for (int m = 0; (m < nMenus) && !foundForThisKey; m++) {
                                                                counter++;

                                                                if (ckey.model == Key.Model.M4)
                                                                    ckey.initPathLookupAll(bombeMenus[m].cribStartPos + bombeMenus[m].cribLen);
                                                                else
                                                                    ckey.initPathLookupHandM3Range(bombeMenus[m].cribStartPos, bombeMenus[m].cribLen);

                                                                for (int i = 0; i < 26; i++) {
                                                                    assumedSteckers[i] = -1;
                                                                    strength[i] = 0;
                                                                }

                                                                if (bombeMenus[m].testIfBombsStops(0, ckey.lookup, assumedSteckers, strength, false)) {
                                                                    foundForThisKey = true;
                                                                    int stb[] = new int[26];
                                                                    for (int i = 0; i < 26; i++)
                                                                        if (assumedSteckers[i] == -1)
                                                                            stb[i] = i;
                                                                        else
                                                                            stb[i] = assumedSteckers[i];
                                                                    ckey.setStecker(stb);
                                                                    tri = ckey.triScoreWithoutLookupBuild(ciphertext, len);
                                                                    ic = ckey.icScoreWithoutLookupBuild(ciphertext, len);
                                                                    ckey.score = scoreForMenus(tri, ic);

                                                                    if (ckey.score > 0) {
                                                                        nStops++;
                                                                        if (nStops == (MAXTOPS - 1)) {
                                                                            CtAPI.printf("\n\nWARNING: Too many stops - Only the top %d keys (sorted by IC and Trigrams) will be kept for Hill Climbing\n", MAXTOPS);
                                                                            CtAPI.print("Search with the current crib parameters (crib string and position/position range) may be inefficient and/or miss the right key.\n");
                                                                            CtAPI.print("It is recommended to either reduce the key range, use a longer crib, or specify fewer positions to search for the crib.\n\n");
                                                                        }
                                                                        boolean sort = false;
                                                                        if (nTops < MAXTOPS) {
                                                                            topKeys[nTops++] = new Key(ckey);
                                                                            sort = true;
                                                                        } else if (ckey.score > topKeys[nTops - 1].score) {
                                                                            topKeys[nTops - 1] = new Key(ckey);
                                                                            sort = true;
                                                                        }

                                                                        if (sort) {
                                                                            for (int i = (nTops - 1); i >= 1; i--)
                                                                                if (topKeys[i].score > topKeys[i - 1].score) {
                                                                                    Key tempKey = topKeys[i];
                                                                                    topKeys[i] = topKeys[i - 1];
                                                                                    topKeys[i - 1] = tempKey;
                                                                                }
                                                                        }
                                                                    }

                                                                    if (ckey.score == bestscore) {
                                                                        counterSameMax++;
                                                                        if (counterSameMax == 100)
                                                                            CtAPI.printf("WARNING: Too many stops with same score (%d). Only stops with higher scores will be displayed\n", bestscore);


                                                                    }

                                                                    if ((ckey.score > bestscore) || ((ckey.score == bestscore) && (counterSameMax < 100))) {
                                                                        if (ckey.score > bestscore)
                                                                            counterSameMax = 0;


                                                                        bestMenu = m;
                                                                        bestscore = ckey.score;

                                                                        printStop(bombeMenus[m], ciphertext, len, ckey, ic, tri, plaintext, assumedSteckers, strength, start, totalKeys, countKeys);

                                                                    }
                                                                } // if valid
                                                            } // for menus
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        if ((nStops == 1) && (debugMenus)) {
            ckey = new Key(from);

            if (ckey.model == Key.Model.M4)
                ckey.initPathLookupRange(bombeMenus[0].cribStartPos, bombeMenus[0].cribLen);
            else
                ckey.initPathLookupHandM3Range(bombeMenus[0].cribStartPos, bombeMenus[0].cribLen);


            for (int i = 0; i < 26; i++) {
                assumedSteckers[i] = -1;
                strength[i] = 0;
            }

            //valid = bombeMenus[0].TestValidity(0,ckey.path_lookup, assumedSteckers, true);
            bombeMenus[0].testIfBombsStops(0, ckey.lookup, assumedSteckers, strength, true);

        }
        long elapsed = (System.currentTimeMillis() - start + 1);


        if (nMenus == 1)
            CtAPI.printf("End of Bombe Search >>%s<< at Position: %d (Turing Score: %.3f Closures: %d Links: %d) \n\nFOUND %d STOP(s) \n\n%d Total Keys Tested in %.1f Seconds(%d/sec)\n\n",
                    Utils.getString(bombeMenus[0].crib, bombeMenus[0].cribLen),
                    bombeMenus[0].cribStartPos, bombeMenus[0].score, bombeMenus[0].totalClosures, bombeMenus[0].totalItems,
                    nStops, counter, elapsed / 1000.0, 1000 * counter / elapsed);
        else if (nStops > 0)
            CtAPI.printf("End of Bombe Search >>%s<< for %d Menus - Best menu found for Position: %d (Turing Score: %.3f Closures: %d Links: %d) \n\nFOUND %d STOP(S) \n %d Total Keys/Menu Combinations Tested in %.1f Seconds(%d/sec)\n\n",
                    Utils.getString(bombeMenus[bestMenu].crib, bombeMenus[bestMenu].cribLen),
                    nMenus,
                    bombeMenus[bestMenu].cribStartPos, bombeMenus[bestMenu].score, bombeMenus[bestMenu].totalClosures, bombeMenus[bestMenu].totalItems,
                    nStops, counter, elapsed / 1000.0, 1000 * counter / elapsed);

        else
            CtAPI.printf("End of Bombe Search >>%s<< for %d Menus \n\nNO STOP FOUND! \n\n%d Keys&Menu Combinations Tested in %.1f Seconds(%d/sec)\n\n",
                    Utils.getString(bombeMenus[0].crib, bombeMenus[0].cribLen),
                    nMenus, counter, elapsed / 1000.0, 1000 * counter / elapsed);


        if ((nTops >= 10) && (hcMaxPass > 0))
            CtAPI.printf("Menu Bombe - Starting batch of %d Keys; Min Score : %d, Median Score: %d, Max Score: %d\n",
                    nTops, topKeys[nTops - 1].score, topKeys[nTops / 2].score, topKeys[0].score);

        if ((nTops > 0) && (hcMaxPass > 0))

            HillClimb.hillClimbBatch(topKeys, nTops, hcMaxPass, THREADS, 10000, ciphertext, len);


    }

    private static void printEstimatedTimeBombeRun(long normalizedNkeys1, int nMenus, MRingScope lRingSettingScope) {
        long normalizedNkeys = normalizedNkeys1;

        int minRate;
        int maxRate;

        if (lRingSettingScope == MRingScope.ALL) {
            minRate = 50000;
            maxRate = 100000;
        } else if (lRingSettingScope == MRingScope.ONE_NON_STEPPING) {
            minRate = 15000;
            maxRate = 30000;
        } else {
            minRate = 25000;
            maxRate = 50000;
        }

        CtAPI.printf("Estimated Search Time: %s for a small number of stops (more if many stops are found)\n\n", Utils.getEstimatedTimeString(nMenus * normalizedNkeys, minRate, maxRate));
    }

    private static void printStop(BombeMenu bombeMenu, byte[] ciphertext, int len, Key ckey, double ic, int tri, byte[] plaintext, byte[] assumedSteckers, byte[] strength, long startTime, long totalKeys, long counterKeys) {
        String plains = "";

        String stbs = "";
        String confirmedSelfS = "";
        String strengthStbs = "";
        String strengthSelfS = "";

        for (int i = 0; i < 26; i++) {

            int s = assumedSteckers[i];
            if (i < s) {
                stbs += "" + Utils.getChar(i) + Utils.getChar(s);

                if (strength[i] > 0)
                    strengthStbs += " " + Utils.getChar(i) + Utils.getChar(s) + "{" + strength[i] + "}";

            } else if (i == s) {
                confirmedSelfS += "" + Utils.getChar(i);
                if (strength[i] > 0)
                    strengthSelfS += " " + Utils.getChar(i) + "{" + strength[i] + "}";
            }

        }

        ckey.setStecker(stbs);
        ckey.encipherDecipherAll(ciphertext, plaintext, len);

        plains = Utils.getString(plaintext, len);
        long elapsed = System.currentTimeMillis() - startTime;
        String desc = String.format("BOMBE [Pos: %3d][%,5dK/%,5dK][%,4dK/sec][%,4d Sec]",
                bombeMenu.cribStartPos, counterKeys / 1000, totalKeys / 1000, counterKeys / elapsed, elapsed / 1000);

        if (BestResults.shouldPushResult(ckey.score)) {
            ReportResult.reportResult(0, ckey, ckey.score, plains, desc);

            CtAPI.printf("MENU STOP NEW BEST - Pos: %d Stop Score: %d (Tri: %d IC: %.5f) - Crib Length: %d, Crib: %s\n",
                    bombeMenu.cribStartPos, ckey.score, tri, ic, bombeMenu.cribLen,
                    Utils.getString(bombeMenu.crib, bombeMenu.cribLen));
            CtAPI.printf("Stecker: [ Pairs: %s (%d) Self: %s (%d) Total: %d ] - Confirmation Strength: Pairs: %s Self: %s\n",
                    stbs, stbs.length(), confirmedSelfS, confirmedSelfS.length(), (stbs.length() + confirmedSelfS.length()),
                    strengthStbs, strengthSelfS);

            ckey.printKeyString("");
            CtAPI.printf("\n%s\n\n", plains);
        }
    }


}
