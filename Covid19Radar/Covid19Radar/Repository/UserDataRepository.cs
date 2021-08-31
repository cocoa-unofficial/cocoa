﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using Covid19Radar.Common;
using Covid19Radar.Model;
using Covid19Radar.Services;
using Covid19Radar.Services.Logs;
using Newtonsoft.Json;

namespace Covid19Radar.Repository
{
    public interface IUserDataRepository
    {
        void SetStartDate(DateTime dateTime);

        DateTime GetStartDate();
        int GetDaysOfUse();

        void RemoveStartDate();

        DateTime GetLastUpdateDate(TermsType termsType);
        void SaveLastUpdateDate(TermsType termsType, DateTime updateDate);

        bool IsAllAgreed();

        void RemoveAllUpdateDate();

        long GetLastProcessTekTimestamp(string region);
        void SetLastProcessTekTimestamp(string region, long created);
        void RemoveLastProcessTekTimestamp();
    }

    public class UserDataRepository : IUserDataRepository
    {
        private readonly IPreferencesService _preferencesService;
        private readonly ILoggerService _loggerService;

        public UserDataRepository(
            IPreferencesService preferencesService,
            ILoggerService loggerService
            )
        {
            _loggerService = loggerService;
            _preferencesService = preferencesService;
        }

        public void SetStartDate(DateTime dateTime)
        {
            _preferencesService.SetValue(PreferenceKey.StartDateTime, dateTime);
        }

        public DateTime GetStartDate()
        {
            return _preferencesService.GetValue(PreferenceKey.StartDateTime, DateTime.UtcNow);
        }

        public int GetDaysOfUse()
        {
            TimeSpan timeSpan = DateTime.UtcNow - GetStartDate();
            return timeSpan.Days;
        }

        public void RemoveStartDate()
        {
            _loggerService.StartMethod();

            _preferencesService.RemoveValue(PreferenceKey.StartDateTime);

            _loggerService.EndMethod();
        }

        public DateTime GetLastUpdateDate(TermsType termsType)
        {
            string key = termsType switch
            {
                TermsType.TermsOfService => PreferenceKey.TermsOfServiceLastUpdateDateTime,
                TermsType.PrivacyPolicy => PreferenceKey.PrivacyPolicyLastUpdateDateTime,
                _ => throw new NotSupportedException()
            };

            var lastUpdateDate = new DateTime(); // 0001/01/01
            if (_preferencesService.ContainsKey(key))
            {
                lastUpdateDate = _preferencesService.GetValue(key, lastUpdateDate);
            }

            return lastUpdateDate;
        }

        public void SaveLastUpdateDate(TermsType termsType, DateTime updateDate)
        {
            _loggerService.StartMethod();

            var key = termsType == TermsType.TermsOfService ? PreferenceKey.TermsOfServiceLastUpdateDateTime : PreferenceKey.PrivacyPolicyLastUpdateDateTime;
            _preferencesService.SetValue(key, updateDate);

            _loggerService.EndMethod();
        }

        public bool IsAllAgreed()
            => _preferencesService.ContainsKey(PreferenceKey.TermsOfServiceLastUpdateDateTime) && _preferencesService.ContainsKey(PreferenceKey.PrivacyPolicyLastUpdateDateTime);

        public void RemoveAllUpdateDate()
        {
            _loggerService.StartMethod();
            _preferencesService.RemoveValue(PreferenceKey.TermsOfServiceLastUpdateDateTime);
            _preferencesService.RemoveValue(PreferenceKey.PrivacyPolicyLastUpdateDateTime);
            _loggerService.EndMethod();
        }

        public long GetLastProcessTekTimestamp(string region)
        {
            _loggerService.StartMethod();
            var result = 0L;
            var jsonString = _preferencesService.GetValue<string>(PreferenceKey.LastProcessTekTimestamp, null);
            if (!string.IsNullOrEmpty(jsonString))
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, long>>(jsonString);
                if (dict.ContainsKey(region))
                {
                    result = dict[region];
                }
            }
            _loggerService.EndMethod();
            return result;
        }

        public void SetLastProcessTekTimestamp(string region, long created)
        {
            _loggerService.StartMethod();
            var jsonString = _preferencesService.GetValue<string>(PreferenceKey.LastProcessTekTimestamp, null);
            Dictionary<string, long> newDict;
            if (!string.IsNullOrEmpty(jsonString))
            {
                newDict = JsonConvert.DeserializeObject<Dictionary<string, long>>(jsonString);
            }
            else
            {
                newDict = new Dictionary<string, long>();
            }
            newDict[region] = created;
            _preferencesService.SetValue(PreferenceKey.LastProcessTekTimestamp, JsonConvert.SerializeObject(newDict));
            _loggerService.EndMethod();
        }

        public void RemoveLastProcessTekTimestamp()
        {
            _loggerService.StartMethod();
            _preferencesService.RemoveValue(PreferenceKey.LastProcessTekTimestamp);
            _loggerService.EndMethod();
        }
    }
}
