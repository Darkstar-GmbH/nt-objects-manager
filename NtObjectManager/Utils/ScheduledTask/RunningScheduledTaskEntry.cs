//  Copyright 2020 Google Inc. All Rights Reserved.
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

using System;

using TaskScheduler = Microsoft.Win32.TaskScheduler.TaskSchedulerSnapshot;
using RegisteredTask = Microsoft.Win32.TaskScheduler.Task;
using ScheduledTask = Microsoft.Win32.TaskScheduler.TaskSchedulerSnapshot;
using ScheduledTaskAction = NtObjectManager.Utils.ScheduledTask.ScheduledTaskAction;
using Action = Microsoft.Win32.TaskScheduler.Action;

using TaskProcessTokenSid = Microsoft.Win32.TaskScheduler.TaskProcessTokenSidType;
using TaskLogonType = Microsoft.Win32.TaskScheduler.TaskLogonType;
using TaskRunLevel = Microsoft.Win32.TaskScheduler.TaskRunLevel;
using TaskActionType = Microsoft.Win32.TaskScheduler.TaskActionType;
using TaskRunFlags = Microsoft.Win32.TaskScheduler.TaskRunFlags;

using Microsoft.Win32.TaskScheduler;
using NtApiDotNet.Win32.Security.Authentication;
using NtApiDotNet.Win32;

namespace NtObjectManager.Utils.ScheduledTask
{
    /// <summary>
    /// State of the running task.
    /// </summary>
    public enum TaskState
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Unknown = 0,
        Disabled,
        Queued,
        Ready,
        Running
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// <para type="description">Class to represent a running scheduled task.</para>
    /// </summary>
    public sealed class RunningScheduledTaskEntry : ScheduledTaskEntry
    {
        /// <summary>
        /// Process ID of the instance.
        /// </summary>
        public int ProcessId { get; }
        /// <summary>
        /// The GUID ID.
        /// </summary>
        public Guid InstanceId { get; }
        /// <summary>
        /// The state of the task.
        /// </summary>
        public TaskState State { get; }
        /// <summary>
        /// The current action.
        /// </summary>
        public string CurrentAction { get; }

        internal RunningScheduledTaskEntry(RunningTask running_task, RegisteredTask task)
            : base(task)
        {
            ProcessId = (int) running_task.EnginePID;
            Guid guid = running_task.InstanceGuid;
            InstanceId = guid;
            State = (TaskState)(int)running_task.State;
            CurrentAction = running_task.CurrentAction;
        }
    }
}
