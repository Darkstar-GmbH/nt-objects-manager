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

using NtApiDotNet;
using NtObjectManager.Cmdlets.Accessible;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.TaskScheduler;

using RegisteredTask = Microsoft.Win32.TaskScheduler.Task;
using Action = Microsoft.Win32.TaskScheduler.Action;

using Principal = System.Security.Principal.IPrincipal;

using NtApiDotNet.Win32;
using System.Security.AccessControl;

namespace NtObjectManager.Utils.ScheduledTask
{
    /// <summary>
    /// Schedled task entry.
    /// </summary>
    public class ScheduledTaskEntry
    {
        /// <summary>
        /// The path to the scheduled task.
        /// </summary>
        public string Path { get; }
        /// <summary>
        /// Is this entry a folder.
        /// </summary>
        public bool Folder { get; }
        /// <summary>
        /// The scheduled task security descriptor.
        /// </summary>
        public SecurityDescriptor SecurityDescriptor { get; }
        /// <summary>
        /// Is the task enabled.
        /// </summary>
        public bool Enabled { get; }
        /// <summary>
        /// Is the task hidden.
        /// </summary>
        public bool Hidden { get; }
        /// <summary>
        /// Can the task be run on demand.
        /// </summary>
        public bool AllowDemandStart { get; }
        /// <summary>
        /// The XML task file.
        /// </summary>
        public string Xml { get; }
        /// <summary>
        /// Principal logon type.
        /// </summary>
        public TaskLogonType LogonType { get; }
        /// <summary>
        /// Principal run level.
        /// </summary>
        public TaskRunLevel RunLevel { get; }
        /// <summary>
        /// Principal name.
        /// </summary>
        public string Principal { get; }
        /// <summary>
        /// Actions for the task.
        /// </summary>
        public IEnumerable<ScheduledTaskAction> Actions { get; }
        /// <summary>
        /// List of required privileges.
        /// </summary>
        public IEnumerable<TaskPrincipalPrivileges> RequiredPrivilege { get; }
        /// <summary>
        /// Principal process token SID.
        /// </summary>
        public TaskProcessTokenSid ProcessTokenSid { get; }
        /// <summary>
        /// Triggers for the task.
        /// </summary>
        public IEnumerable<ScheduledTaskTrigger> Triggers { get; }
        /// <summary>
        /// Indicates whether the task has action arguments.
        /// </summary>
        public bool HasActionArguments { get; }

        internal ScheduledTaskEntry(RegisteredTask task)
        {
            SecurityDescriptor = new SecurityDescriptor(
                task.Folder.GetAccessControl(AccessControlSections.All).GetSecurityDescriptorBinaryForm());

            Path = task.Path;
            Enabled = task.Enabled;
            Xml = task.Xml;
            var definition = task.Definition;
            var settings = definition.Settings;
            Hidden = settings.Hidden;
            AllowDemandStart = settings.AllowDemandStart;
            var principal = definition.Principal;
            if (principal.RunLevel.Equals(TaskRunLevel.Highest))
            {
                RunLevel = TaskRunLevel.Highest;
            }

            List<TaskPrincipalPrivileges> privs = new List<TaskPrincipalPrivileges>();
            privs.AddRange(Enumerable.Range(0, principal.RequiredPrivileges.Count())
                .Select(i => principal.RequiredPrivileges));

            ProcessTokenSid = (TaskProcessTokenSid) (int) principal.ProcessTokenSidType;
            RequiredPrivilege = privs.AsReadOnly();

            TaskLogonType logon_type = TaskLogonType.None;
            string principal_name = string.Empty;
            if (principal.LogonType.Equals(TaskLogonType.Group)) {
                logon_type = TaskLogonType.Group;
                principal_name = principal.GroupId;
            }
            else if (principal.LogonType.Equals(SecurityLogonType.Interactive) ||
                principal.LogonType.Equals(Microsoft.Win32.TaskScheduler.TaskLogonType.Password) ||
                principal.LogonType.Equals(Microsoft.Win32.TaskScheduler.TaskLogonType.InteractiveTokenOrPassword)) {
                logon_type = TaskLogonType.User;
                principal_name = principal.UserId;
            }
            else if (principal.LogonType.Equals(TaskLogonType.ServiceAccount)) {
                logon_type = TaskLogonType.ServiceAccount;
                principal_name = principal.UserId;
            }
            else if (principal.LogonType.Equals(TaskLogonType.S4U)) {
                logon_type = TaskLogonType.S4U;
                principal_name = principal.UserId;
            }

            LogonType = logon_type;
            Principal = principal_name;
            Actions = definition.Actions.Cast<Action>().Select(a => new ScheduledTaskAction(a)).ToList().AsReadOnly();
            HasActionArguments = Actions.Any(a => a.HasArguments);
            Triggers = definition.Triggers
                .Cast<Trigger>()
                .Select(trigger => ScheduledTaskTrigger.Create(trigger))
                .ToList()
                .AsReadOnly()
                ;
        }

        internal ScheduledTaskEntry(TaskFolder folder)
        {
            Folder = true;
            SecurityDescriptor = new SecurityDescriptor(
                folder.GetAccessControl(AccessControlSections.All).GetSecurityDescriptorBinaryForm());

            Path = folder.Path;
        }

        internal CommonAccessCheckResult CreateResult(AccessMask granted_access, TokenInformation token_info)
        {
            if (Folder)
            {
                return new CommonAccessCheckResult(Path, "Scheduled Task", granted_access, _file_type.GenericMapping,
                    SecurityDescriptor.Clone(), typeof(FileDirectoryAccessRights), true, token_info);
            }
            else
            {
                return new CommonAccessCheckResult(
                    this.Principal, typeof(Principal).GetType().Name, 
                    granted_access, _file_type.GenericMapping, 
                    SecurityDescriptor.Clone(), typeof(AccessMask), 
                    _file_type == NtType.GetTypeByType<NtDirectory>(), token_info);
            }
        }

        private static readonly NtType _file_type = NtType.GetTypeByType<NtFile>();
    }
}
