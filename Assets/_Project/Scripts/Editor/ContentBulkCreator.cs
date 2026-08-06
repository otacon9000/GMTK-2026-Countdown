using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GmtkCountdown.Editor
{
    public static class ContentBulkCreator
    {
        private const string PromptsFolder = "Assets/_Project/ScriptableObjects/Prompts";
        private const string FragmentsFolder = "Assets/_Project/ScriptableObjects/Fragments";
        private const int MaxNameLength = 40;

        private static readonly string[] PromptsToCreate =
        {
            "I haven't finished the report because {0}.",
            "The project is delayed because {0}.",
            "I missed the deadline because {0}.",
            "I'm still at my desk because {0}.",
            "The email never got sent because {0}.",
            "I couldn't join the meeting because {0}.",
            "The presentation isn't ready because {0}.",
            "I haven't replied to your messages because {0}.",
            "The numbers don't add up because {0}.",
            "I'm running late again because {0}.",
            "The file got corrupted because {0}.",
            "I forgot, because {0}.",
            "It's not my fault — it's because {0}.",
            "The client is upset because {0}.",
            "I can explain everything: {0}.",
            "This is taking longer than expected because {0}.",
            "I was going to tell you, but {0}.",
            "Nothing is working today because {0}.",
            "I promise I was working, but {0}.",
            "The whole team is behind because {0}.",
        };

        private static readonly (string fragmentText, FragmentCategory category, int baseEarnedTime)[] FragmentsToCreate =
        {
            ("my laptop update took four hours", FragmentCategory.Technology, 25),
            ("the wifi went down again", FragmentCategory.Technology, 25),
            ("I got locked out of my own account", FragmentCategory.Technology, 15),
            ("the software update deleted everything", FragmentCategory.Technology, 28),
            ("my computer installed updates mid-task", FragmentCategory.Technology, 20),
            ("the printer jammed for the fifth time", FragmentCategory.Technology, 10),
            ("my mouse battery died at the worst moment", FragmentCategory.Technology, 8),
            ("the cloud sync got stuck at 99%", FragmentCategory.Technology, 18),

            ("I had a sudden headache", FragmentCategory.Health, 15),
            ("I twisted my wrist typing too fast", FragmentCategory.Health, 10),
            ("I felt dizzy after lunch", FragmentCategory.Health, 18),
            ("my allergies flared up out of nowhere", FragmentCategory.Health, 20),
            ("I pulled a muscle reaching for coffee", FragmentCategory.Health, 8),
            ("I needed to lie down for ten minutes", FragmentCategory.Health, 12),
            ("my eyes wouldn't focus on the screen", FragmentCategory.Health, 15),
            ("I think I'm coming down with something", FragmentCategory.Health, 22),

            ("my cat walked across my keyboard", FragmentCategory.Family, 12),
            ("my kid needed help with homework", FragmentCategory.Family, 25),
            ("my neighbor's alarm wouldn't stop", FragmentCategory.Family, 15),
            ("a family emergency came up", FragmentCategory.Family, 28),
            ("my parents called about something urgent", FragmentCategory.Family, 20),
            ("my dog ate an important document", FragmentCategory.Family, 10),
            ("my sibling borrowed my charger", FragmentCategory.Family, 8),
            ("someone needed a ride unexpectedly", FragmentCategory.Family, 18),

            ("there was a citywide power outage", FragmentCategory.ForceMajeure, 30),
            ("the building's fire alarm went off", FragmentCategory.ForceMajeure, 28),
            ("a pipe burst on my floor", FragmentCategory.ForceMajeure, 25),
            ("there was a sudden thunderstorm", FragmentCategory.ForceMajeure, 15),
            ("the elevator got stuck with me in it", FragmentCategory.ForceMajeure, 20),
            ("road construction blocked my street", FragmentCategory.ForceMajeure, 18),
            ("a flock of pigeons invaded the office", FragmentCategory.ForceMajeure, 8),
            ("the office coffee machine exploded", FragmentCategory.ForceMajeure, 12),

            ("I was abducted by a very polite alien", FragmentCategory.Absurd, 5),
            ("I got lost in my own thoughts", FragmentCategory.Absurd, 10),
            ("a time paradox delayed me", FragmentCategory.Absurd, 5),
            ("I was practicing my resignation speech", FragmentCategory.Absurd, 8),
            ("I was calculating the meaning of life", FragmentCategory.Absurd, 6),
            ("my houseplant needed emotional support", FragmentCategory.Absurd, 8),
            ("I was training for the paperclip Olympics", FragmentCategory.Absurd, 6),
            ("the universe simply wasn't ready for this task", FragmentCategory.Absurd, 5),
        };

        [MenuItem("Tools/GMTK Countdown/Generate Prompt Data Batch")]
        private static void GeneratePromptDataBatch()
        {
            BulkCreatorUtility.EnsureFolder("Assets/_Project/ScriptableObjects", "Prompts");

            var createdPaths = new List<string>();
            var skippedPaths = new List<string>();
            var pathsUsedThisRun = new HashSet<string>();

            foreach (string promptText in PromptsToCreate)
            {
                string baseFileName = "Prompt_" + BulkCreatorUtility.SanitizeName(promptText, MaxNameLength);
                string assetPath = BulkCreatorUtility.ResolveUniquePath(PromptsFolder, baseFileName, pathsUsedThisRun);

                if (AssetDatabase.LoadAssetAtPath<PromptData>(assetPath) != null)
                {
                    skippedPaths.Add(assetPath);
                    continue;
                }

                var promptData = ScriptableObject.CreateInstance<PromptData>();
                var serializedObject = new SerializedObject(promptData);
                serializedObject.FindProperty("promptText").stringValue = promptText;
                serializedObject.ApplyModifiedProperties();

                AssetDatabase.CreateAsset(promptData, assetPath);
                createdPaths.Add(assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            LogSummary("PromptData", PromptsFolder, createdPaths, skippedPaths);
        }

        [MenuItem("Tools/GMTK Countdown/Generate Fragment Data Batch")]
        private static void GenerateFragmentDataBatch()
        {
            BulkCreatorUtility.EnsureFolder("Assets/_Project/ScriptableObjects", "Fragments");

            var createdPaths = new List<string>();
            var skippedPaths = new List<string>();
            var pathsUsedThisRun = new HashSet<string>();

            foreach (var (fragmentText, category, baseEarnedTime) in FragmentsToCreate)
            {
                string baseFileName = "Fragment_" + BulkCreatorUtility.SanitizeName(fragmentText, MaxNameLength);
                string assetPath = BulkCreatorUtility.ResolveUniquePath(FragmentsFolder, baseFileName, pathsUsedThisRun);

                if (AssetDatabase.LoadAssetAtPath<FragmentData>(assetPath) != null)
                {
                    skippedPaths.Add(assetPath);
                    continue;
                }

                var fragmentData = ScriptableObject.CreateInstance<FragmentData>();
                var serializedObject = new SerializedObject(fragmentData);
                serializedObject.FindProperty("fragmentText").stringValue = fragmentText;
                serializedObject.FindProperty("category").enumValueIndex = (int)category;
                serializedObject.FindProperty("baseEarnedTime").intValue = baseEarnedTime;
                serializedObject.ApplyModifiedProperties();

                AssetDatabase.CreateAsset(fragmentData, assetPath);
                createdPaths.Add(assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            LogSummary("FragmentData", FragmentsFolder, createdPaths, skippedPaths);
        }

        private static void LogSummary(string assetTypeName, string folder, List<string> createdPaths, List<string> skippedPaths)
        {
            var summary = new StringBuilder();
            summary.AppendLine($"ContentBulkCreator: created {createdPaths.Count} {assetTypeName} asset(s) in {folder}.");
            if (skippedPaths.Count > 0)
            {
                summary.AppendLine($"Skipped {skippedPaths.Count} already-existing asset(s):");
                foreach (string skipped in skippedPaths)
                {
                    summary.AppendLine($"  - {skipped}");
                }
            }

            Debug.Log(summary.ToString());
        }
    }
}
