using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine;
namespace GEGFramework {
    /// <summary>
    /// Responsible for computing a new difficulty level (score)
    /// </summary>
    class GEGDifficultyManager {

        int prevDiff; // Previous difficulty level
        int currentRounds;  // Counting rounds for staying at the current difficulty level:
        Dictionary<string, List<int>> propertyScore;
        Dictionary<string, List<int>> propertyLevelTable;

        /// <summary>
        /// GEGScoreManager constructor
        /// </summary>
        /// <param name="defaultDiff">Initial difficulty level</param>
        public GEGDifficultyManager(int defaultDiff) {
            if (defaultDiff < 0 || defaultDiff > 10)
                throw new ArgumentOutOfRangeException("Default difficulty level must be between 0 and 10.");
            currentRounds = 0;
            prevDiff = defaultDiff;
        }


        /// <summary>
        /// Returns a difficulty level (score) based on the inputs
        /// </summary>
        /// <param name="zeroDuration">Desired zero difficulty duration, counted in diffEval rounds</param>
        /// <param name="lowDuration">Desired low difficulty duration</param>
        /// <param name="peakDuration">Desired peak duration</param>
        /// <returns></returns>
        public int GetDifficulty(int zeroDuration, int lowDuration, int peakDuration)
        {

            int newDiff = prevDiff; // New difficulty level to return
            currentRounds++; // Means 10 seconds passed if GEGPackedData.diffEvalInterval = 10f

            // Cases for difficulty equals to 0 where no enemy should show up
            if (prevDiff == 0)
            {
                // Enough time to relax, so we set difficulty to very low level including 1, 2, 3. 
                if (currentRounds > zeroDuration)
                {
                    currentRounds = 0;
                    newDiff = Random.Range(1, 4);
                }
                // Else not enough time to relax, so we continue 0 difficulty so do nothing
            }

            // Cases for low difficulty: from 1 to 6
            else if (prevDiff > 0 && prevDiff < 7)
            {
                // Enough time for low level difficulty, so we switch to high level difficulty.
                if (currentRounds > lowDuration)
                {
                    currentRounds = 0;
                    newDiff = 7;
                }
                else
                { // Not enough time in low level
                    //Not reached the highest level for low difficulty yet, so we make the difficulty a little bit higher
                    if (prevDiff < 6)
                    {
                        newDiff += 1;
                    }
                }
            }

            // Cases for high difficulty: 7,8,9
            else if (prevDiff >= 7)
            {
                // enough peak time, go to relax mode
                if (currentRounds > peakDuration)
                {
                    currentRounds = 0;
                    newDiff = 0;
                }
                else
                { // The peak time is not long enough
                    int continuePeakOrNot = Random.Range(0, 10);

                    if (continuePeakOrNot >= 7)
                    { // 30% new difficulty = 0
                        currentRounds = 0;
                        newDiff = 0;
                    }
                    else
                    { // 70% increase difficulty
                        if (prevDiff < 9)
                        {
                            newDiff += 1;
                        }
                    }
                }
            }

            prevDiff = newDiff;
            return newDiff;
        }


        /// <summary>
        /// Generate enemy properties.
        /// </summary>
        /// <param name="difflevel">Difficulty level (from 0 to 10)</param>
        /// <param name="PropertyList">Dictionary contains all attributes with enable or not</param>
        /// <returns></returns>
        void EnemyPropertyGenerator(int difflevel)
        {
            List<KeyValuePair<string, int>> results = new List<KeyValuePair<string, int>>();

            foreach (GEGCharacter character in GEGPackedData.characters)
            {
                if (character.type == GEGCharacterType.Enemy)
                {
                    foreach (GEGCharacterProperty prop in character.propSO)
                    {
                        if (prop.diffEnabled)
                        {
                            prop.value = prop.defaultValue + prop.defaultValue * difflevel / 10;
                            //results.Add();
                            //kvp.UpdateProperty(difflevel);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate enemy number.
        /// </summary>
        /// <param name="difflevel">Difficulty level (from 0 to 10)</param>
        /// <returns></returns>
        void EnemyNumberGenerator(int difflevel)
        {
            List<KeyValuePair<string, float>> temp = new List<KeyValuePair<string, float>>();
            float totalDiffcult = 0;
            foreach (GEGCharacter character in GEGPackedData.characters)
            {
                if (character.type == GEGCharacterType.Enemy)
                {
                    temp.Add(new KeyValuePair<string, float>(character.Name, character.diffFactor));
                    totalDiffcult += character.diffFactor;
                }
            }
            int upperBound = Mathf.CeilToInt(temp.Count * difflevel / 10);
            /*foreach (GEGTypeContainer enemy in GEGPackedData.enemyTypeData) {
                temp.Add(new KeyValuePair<string, float>(enemy.name, enemy.diffFactor));
            }*/
            temp.Sort((a, b) => a.Value.CompareTo(b.Value)); //sort enemy by diffFactor

            for (int i = 0; i < upperBound; i++)
            {
                GEGPackedData.characters[upperBound - i - 1].nextWaveNum = (int)(difflevel * temp[upperBound - i - 1].Value * 10);
            }

        }
        /// <summary>
        /// Generate enemy number.
        /// </summary>
        /// <param name="difflevel">Difficulty level (from 0 to 10)</param>
        /// <returns></returns>
        void EnemyNumberUpdate(int difflevel)
        {
            List<KeyValuePair<string, float>> temp = new List<KeyValuePair<string, float>>();
            float totalDiffcult = 0;
            foreach (GEGCharacter character in GEGPackedData.characters)
            {
                if (character.type == GEGCharacterType.Enemy)
                {
                    temp.Add(new KeyValuePair<string, float>(character.Name, character.diffFactor));
                    totalDiffcult += character.diffFactor;
                }
            }
            int upperBound = Mathf.CeilToInt(temp.Count * difflevel / 10);
            temp.Sort((a, b) => a.Value.CompareTo(b.Value)); //sort enemy by diffFactor
            int tn = 0;
            float totaldiffseed = difflevel * difflevel * Mathf.Log(difflevel, 2) + 1;
            Debug.Log(upperBound - 1);
            for (int i = 0; i < upperBound - 1; i++)
            {
                float a = Random.Range(temp[upperBound - i - 1].Value, totaldiffseed * 2 / 3);
                tn = Mathf.FloorToInt(a / temp[upperBound - i - 1].Value);
                GEGPackedData.characters[upperBound - i - 1].nextWaveNum = tn;
                totaldiffseed = totaldiffseed - tn * temp[upperBound - i - 1].Value;
            }
            tn = Mathf.RoundToInt(totaldiffseed / temp[0].Value);
            GEGPackedData.characters[0].nextWaveNum = tn;
        }




    }

}