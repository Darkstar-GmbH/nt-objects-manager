//  Copyright 2019 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using Microsoft.Win32.TaskScheduler;

namespace NtObjectManager.Utils.ScheduledTask
{
    /// <summary>
    /// Class to represent a scheduled task action.
    /// </summary>
    public class ScheduledTaskAction
    {
        /// <summary>
        /// The ID of the action.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Type of action.
        /// </summary>
        public TaskActionType ActionType { get; }

        /// <summary>
        /// Summary of what will be invoked.
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Indicates if this action takes arguments.
        /// </summary>
        public bool HasArguments { get; }

        /// <summary>
        /// Overridden ToString.
        /// </summary>
        /// <returns>The action as a string.</returns>
        public override string ToString()
        {
            return $"{ActionType}: {Action}";
        }

        internal ScheduledTaskAction(Action action)
        {
            Id = action.Id ?? string.Empty;
            Action = string.Empty;
            switch ((ushort) action.ActionType)
            {
                case (ushort) TaskActionType.Execute:
                    ActionType = TaskActionType.Execute;
                    if (action is ExecAction exec_action)
                    {
                        Action = $"{exec_action.Path} {exec_action.Arguments}";
                    }
                    break;
                case (ushort) TaskActionType.ComObject:
                    ActionType = TaskActionType.ComObject;
                    if (action is ComHandlerAction com_action)
                    {
                        Action = $"{com_action.ClassId:B} {com_action.Data}";
                    }
                    break;
                case (ushort) TaskActionType.SendEmail:
                    ActionType = TaskActionType.SendEmail;
                    if (action is EmailAction email_action)
                    {
                        Action = $"From: {email_action.From} To: {email_action.To}";
                    }
                    break;
                case (ushort) TaskActionType.ShowMessage:
                    ActionType = TaskActionType.ShowMessage;
                    if (action is ShowMessageAction msg_action)
                    {
                        Action = $"Title: {msg_action.Title} Body: {msg_action.MessageBody}";
                    }
                    break;
            }
            HasArguments = Action?.Contains("$(Arg") ?? false;
        }
    }
}
