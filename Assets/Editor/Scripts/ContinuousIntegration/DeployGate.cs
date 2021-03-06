﻿using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityModule.Settings;

namespace ContinuousIntegration
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class DeployGate
    {
        /// <summary>
        /// 環境変数キー: ユーザ
        /// </summary>
        private const string EnvironmentKeyCommandDeployGate = "COMMAND_DEPLOY_GATE";

        /// <summary>
        /// DeployGate に配信します
        /// </summary>
        /// <param name="archivePath">配信対象のアーカイブファイルのパス</param>
        /// <param name="message">アップロードするファイルの説明</param>
        public static void Deploy(string archivePath, string message = null)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = DeployGateSetting.GetOrDefault().CommandDeployGate,
                    Arguments = GenerateArguments(archivePath, message),
                    CreateNoWindow = true,
                    UseShellExecute = true,
                }
            };
            process.Start();
            process.WaitForExit();
            var exitCode = process.ExitCode; // Close前にExitCode取得しないとエラー
            process.Close();

            if (exitCode != 0 && Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }

        /// <summary>
        /// dg コマンドへ渡す引数を生成する
        /// </summary>
        /// <param name="archivePath">配信対象のアーカイブファイルのパス</param>
        /// <param name="message">アップロードするファイルの説明</param>
        /// <returns>dg コマンドへ渡す引数</returns>
        private static string GenerateArguments(string archivePath, string message)
        {
            var arguments = $"deploy \"{archivePath}\"";
            if (!string.IsNullOrEmpty(message))
            {
                arguments += $" -m \"{message}\"";
            }

            return arguments;
        }
    }
}