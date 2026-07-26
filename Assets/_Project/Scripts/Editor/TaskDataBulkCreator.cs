using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GmtkCountdown.Editor
{
    public static class TaskDataBulkCreator
    {
        private const string TargetFolder = "Assets/_Project/ScriptableObjects/Tasks";
        private const int MaxNameLength = 40;

        private static readonly (string taskName, int requiredTime, int scoreValue)[] TasksToCreate =
        {
            ("Sort the Paperclips by Color", 20, 10),
            ("Reply \"Got It\" to 200 Unread Emails", 25, 15),
            ("Find the Perfect Font for a Button No One Will Click", 30, 15),
            ("Explain to a Client What \"Syncing\" Means", 30, 20),
            ("Restart the Router Because It Always Needs Restarting", 25, 15),
            ("Update the Slide Deck by Changing Only the Font", 35, 20),
            ("Convince HR Your Lateness Is \"Creative Flexibility\"", 30, 20),
            ("Run an A/B Test on Two Identical Shades of Grey", 40, 25),
            ("Move a Task from \"To Do\" to \"In Progress\" and Close It", 20, 15),
            ("Convince HR That Coffee Is a Company Tool", 35, 25),
            ("Refactor a Single Code Comment", 45, 30),
            ("Prepare a Forecast Based on a Random Number", 50, 35),
            ("Rename \"Final_Document_v2_DEFINITELY_ok\" for the Tenth Time", 30, 20),
            ("Convince Legal That the Emoji in the Email Isn't Binding", 45, 30),
            ("Present a Roadmap That Changes the Moment You Finish Presenting It", 55, 40),
            ("Explain the Cloud to the Boss Using Only Wrong Metaphors", 50, 35),
            ("Optimize a Process Nobody Uses Anymore", 45, 30),
            ("Convince Sales That \"Almost Closed\" Isn't a Real Status", 60, 45),
            ("Brainstorm Alone and Vote for Your Own Idea", 40, 25),
            ("Justify an Expense Report for a Team Building That Never Happened", 65, 50),
        };

        [MenuItem("Tools/GMTK Countdown/Generate Task Data Batch")]
        private static void GenerateTaskDataBatch()
        {
            if (!AssetDatabase.IsValidFolder(TargetFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Tasks");
            }

            var createdNames = new List<string>();
            var skippedNames = new List<string>();

            foreach (var (taskName, requiredTime, scoreValue) in TasksToCreate)
            {
                string baseFileName = "Task_" + BulkCreatorUtility.SanitizeName(taskName, MaxNameLength);
                string assetPath = $"{TargetFolder}/{baseFileName}.asset";

                if (AssetDatabase.LoadAssetAtPath<TaskData>(assetPath) != null)
                {
                    skippedNames.Add(assetPath);
                    continue;
                }

                var taskData = ScriptableObject.CreateInstance<TaskData>();
                var serializedObject = new SerializedObject(taskData);
                serializedObject.FindProperty("taskName").stringValue = taskName;
                serializedObject.FindProperty("requiredTime").intValue = requiredTime;
                serializedObject.FindProperty("scoreValue").intValue = scoreValue;
                serializedObject.ApplyModifiedProperties();

                AssetDatabase.CreateAsset(taskData, assetPath);
                createdNames.Add(assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var summary = new StringBuilder();
            summary.AppendLine($"TaskDataBulkCreator: created {createdNames.Count} TaskData asset(s) in {TargetFolder}.");
            if (skippedNames.Count > 0)
            {
                summary.AppendLine($"Skipped {skippedNames.Count} already-existing asset(s):");
                foreach (var skipped in skippedNames)
                {
                    summary.AppendLine($"  - {skipped}");
                }
            }

            Debug.Log(summary.ToString());
        }
    }
}
