/*
 * PhoneGap is available under *either* the terms of the modified BSD license *or* the
 * MIT License (2008). See http://opensource.org/licenses/alphabetical for full text.
 *
 * Copyright (c) 2005-2011, Nitobi Software Inc.
 * Copyright (c) 2011, Microsoft Corporation
 */

using System.Runtime.Serialization;
using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.Cordova.JSON;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.Controls;
using System.Windows;
using Microsoft.Phone.Tasks;
using System.Diagnostics;

namespace WPCordovaClassLib.Cordova.Commands
{
    /// <summary>
    /// Represents command that allows the user to choose a date (day/month/year) or time (hour/minute/am/pm).
    /// </summary>
    public class DateTimePicker : BaseCommand
    {

        private string _callbackId;
        public event EventHandler<PluginResult> mySavedHandler;

        #region DateTimePicker Options

        /// <summary>
        /// Represents DateTimePicker options
        /// </summary>
        [DataContract]
        public class DateTimePickerOptions
        {
            /// <summary>
            /// Initial value for time or date
            /// </summary>
            [DataMember(IsRequired = false, Name = "value")]
            public DateTime Value { get; set; }   
         
                        /// <summary>
            /// Creates options object with default parameters
            /// </summary>
            public DateTimePickerOptions()
            {
                this.SetDefaultValues(new StreamingContext());
            }

            /// <summary>
            /// Initializes default values for class fields.
            /// Implemented in separate method because default constructor is not invoked during deserialization.
            /// </summary>
            /// <param name="context"></param>
            [OnDeserializing()]
            public void SetDefaultValues(StreamingContext context)
            {
                this.Value = DateTime.Now;
            }

        }
        #endregion

        /// <summary>
        /// Used to open datetime picker
        /// </summary>
        private DateTimePickerTask dateTimePickerTask;

        /// <summary>
        /// DateTimePicker options
        /// </summary>
        private DateTimePickerOptions dateTimePickerOptions;

        /// <summary>
        /// Triggers  special UI to select a date (day/month/year)
        /// </summary>
        public void selectDate(string options)
        {
            try {           
                try
                {
                    var args = JSON.JsonHelper.Deserialize<string[]>(options);
                    _callbackId = args[args.Length - 1];
                    if (ResultHandlers.ContainsKey(_callbackId))
                    {
                        mySavedHandler = ResultHandlers[_callbackId];
                    }
                    string value = WPCordovaClassLib.Cordova.JSON.JsonHelper.Deserialize<string[]>(options)[0];
                    dateTimePickerOptions = new DateTimePickerOptions();
                    if(!String.IsNullOrEmpty(value)) {
                        dateTimePickerOptions.Value = FromUnixTime(long.Parse(value));
                    }
                }
                catch (Exception ex)
                {
                    this.DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, ex.Message));
                    return;
                }

                this.dateTimePickerTask = new DateTimePickerTask();
                dateTimePickerTask.Value = dateTimePickerOptions.Value;
                dateTimePickerTask.Completed += this.dateTimePickerTask_Completed;
                dateTimePickerTask.Show(DateTimePickerTask.DateTimePickerType.Date);
            }
            catch (Exception e)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, e.Message));
            }
        }

        /// <summary>
        /// Triggers  special UI to select a time (hour/minute/am/pm).
        /// </summary>
        public void selectTime(string options)
        {
            try
            {
                try
                {
                    var args = JSON.JsonHelper.Deserialize<string[]>(options);
                    _callbackId = args[args.Length - 1];
                    if (ResultHandlers.ContainsKey(_callbackId))
                    {
                        mySavedHandler = ResultHandlers[_callbackId];
                    }
                    string value = WPCordovaClassLib.Cordova.JSON.JsonHelper.Deserialize<string[]>(options)[0];
                    dateTimePickerOptions = new DateTimePickerOptions();
                    if (!String.IsNullOrEmpty(value)) {
                        dateTimePickerOptions.Value = FromUnixTime(long.Parse(value));
                    }
                }
                catch (Exception ex)
                {
                    this.DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, ex.Message));
                    return;
                }

                this.dateTimePickerTask = new DateTimePickerTask();
                dateTimePickerTask.Value = dateTimePickerOptions.Value;
                dateTimePickerTask.Completed += this.dateTimePickerTask_Completed;
                dateTimePickerTask.Show(DateTimePickerTask.DateTimePickerType.Time);
            }
            catch (Exception e)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, e.Message));
            }
        }

        private DateTime FromUnixTime(long unixtime) {
            // Unix timestamp is seconds past epoch
            return  new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(unixtime).ToLocalTime();
        }


        /// <summary>
        /// Handles datetime picker result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">stores information about current captured image</param>
        private void dateTimePickerTask_Completed(object sender, DateTimePickerTask.DateTimeResult e)
        {
            if (e.Error != null)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR));
                return;
            }
            if (!ResultHandlers.ContainsKey(_callbackId))
            {
                ResultHandlers.Add(_callbackId, mySavedHandler);
            }
            switch (e.TaskResult)
            {
                case TaskResult.OK:
                    try
                    {
                        long result = (long) e.Value.Value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        DispatchCommandResult(new PluginResult(PluginResult.Status.OK, result.ToString()), _callbackId);
                    }
                    catch (Exception ex)
                    {
                        DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "DATE PICKER ERROR: " + ex.Message), _callbackId);
                    }
                    break;

                case TaskResult.Cancel:
                    DispatchCommandResult(new PluginResult(PluginResult.Status.OK, null), _callbackId);
                    break;               
            }
            this.dateTimePickerTask = null;
        }       

    }
}