﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityModule.Settings;

namespace ContinuousIntegration {

    // ReSharper disable once PartialTypeWithSinglePart
    public partial class DeployGate {

        public const int POSTPROCESS_BUILD_CALLBACK_ORDER = 200;

        public class PostprocessBuild : IPostprocessBuild {

            /// <summary>
            /// ビルドパラメータ: ユーザ
            /// </summary>
            private const string BUILD_PARAMETER_USER = "BUILD_USER";

            /// <summary>
            /// ビルドパラメータ: ブランチ
            /// </summary>
            private const string BUILD_PARAMETER_BRANCH = "BUILD_BRANCH";

            /// <summary>
            /// ビルドパラメータ: 環境
            /// </summary>
            private const string BUILD_PARAMETER_ENVIRONMENT = "BUILD_ENVIRONMENT";

            /// <summary>
            /// ビルドパラメータ: エディタバージョン
            /// </summary>
            private const string BUILD_PARAMETER_EDITOR_VERSION = "BUILD_EDITOR_VERSION";

            /// <summary>
            /// メッセージ接頭辞
            /// </summary>
            private static readonly Dictionary<string, string> MESSAGE_PREFIXES = new Dictionary<string, string>() {
                { BUILD_PARAMETER_USER,           "User" },
                { BUILD_PARAMETER_BRANCH,         "Branch" },
                { BUILD_PARAMETER_ENVIRONMENT,    "Environment" },
                { BUILD_PARAMETER_EDITOR_VERSION, "Unity" },
            };

            public int callbackOrder {
                get {
                    return POSTPROCESS_BUILD_CALLBACK_ORDER;
                }
            }

            public void OnPostprocessBuild(BuildTarget target, string path) {
                Deploy(ResolveArchivePath(target, path), GenerateMessage());
            }

            /// <summary>
            /// ビルド済のアーカイブファイルのパスを解決
            /// </summary>
            /// <param name="target">出力先ターゲットプラットフォーム</param>
            /// <param name="path">出力先パス</param>
            /// <returns>解決済みのパス</returns>
            /// <exception cref="ArgumentException">ファイルが見付からなかった場合に throw</exception>
            private static string ResolveArchivePath(BuildTarget target, string path) {
                string archivePath = string.Empty;
                switch (target) {
                    case BuildTarget.iOS:
                        archivePath = string.Format("{0}/build/Unity-iPhone.ipa", path);
                        break;
                    case BuildTarget.Android:
                        archivePath = path;
                        break;
                }
                if (!File.Exists(archivePath)) {
                    throw new ArgumentException(string.Format("\"{0}\" にビルド済のアーカイブが見付かりませんでした。", archivePath));
                }
                return archivePath;
            }

            /// <summary>
            /// メッセージを生成する
            /// </summary>
            /// <returns>メッセージ</returns>
            private static string GenerateMessage() {
                string message = string.Empty;
                message += GenerateBuildMessage(BUILD_PARAMETER_USER);
                message += GenerateBuildMessage(BUILD_PARAMETER_BRANCH);
                message += GenerateCommitMessage();
                message += GenerateBuildMessage(BUILD_PARAMETER_ENVIRONMENT);
                message += GenerateBuildMessage(BUILD_PARAMETER_EDITOR_VERSION);
                return message;
            }

            /// <summary>
            /// 指定されたビルドパラメータに該当するメッセージを生成して返却する
            /// </summary>
            /// <returns>メッセージ</returns>
            private static string GenerateBuildMessage(string buildParameter) {
                string value = Environment.GetEnvironmentVariable(buildParameter);
                if (string.IsNullOrEmpty(value)) {
                    return string.Empty;
                }
                return string.Format("{0}: {1}\n", MESSAGE_PREFIXES[buildParameter], value);
            }

            /// <summary>
            /// コミット番号のメッセージを生成して返却する
            /// </summary>
            /// <returns>メッセージ</returns>
            private static string GenerateCommitMessage() {
                return string.Format("Commit: {0}\n", GetCommit());
            }

            /// <summary>
            /// コミット番号を取得する
            /// </summary>
            /// <returns>メッセージ</returns>
            private static string GetCommit() {
                System.Diagnostics.Process process = new System.Diagnostics.Process {
                    StartInfo = {
                        FileName = "/usr/local/bin/git",
                        Arguments = "rev-parse HEAD",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                string commit = process.StandardOutput.ReadToEnd().TrimEnd();
                process.Close();
                return commit;
            }

        }

    }

}