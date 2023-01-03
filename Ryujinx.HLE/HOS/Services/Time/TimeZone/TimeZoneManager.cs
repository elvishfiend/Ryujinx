﻿using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.Utilities;
using System.IO;
using static Ryujinx.HLE.HOS.Services.Time.TimeZone.TimeZoneRule;

namespace Ryujinx.HLE.HOS.Services.Time.TimeZone
{
    class TimeZoneManager
    {
        private bool                 _isInitialized;
        private TimeZoneRule         _myRules;
        private string               _deviceLocationName;
        private UInt128              _timeZoneRuleVersion;
        private uint                 _totalLocationNameCount;
        private SteadyClockTimePoint _timeZoneUpdateTimePoint;
        private object               _lock;

        public TimeZoneManager()
        {
            _isInitialized       = false;
            _deviceLocationName  = "UTC";
            _timeZoneRuleVersion = new UInt128();
            _lock                = new object();

            // Empty rules
            _myRules = new TimeZoneRule
            {
                Ats   = new long[TzMaxTimes],
                Types = new byte[TzMaxTimes],
                Ttis  = new TimeTypeInfo[TzMaxTypes],
                Chars = new char[TzCharsArraySize]
            };

            _timeZoneUpdateTimePoint = SteadyClockTimePoint.GetRandom();
        }

        public bool IsInitialized()
        {
            bool res;

            lock (_lock)
            {
                res = _isInitialized;
            }

            return res;
        }

        public void MarkInitialized()
        {
            lock (_lock)
            {
                _isInitialized = true;
            }
        }

        public ResultCode GetDeviceLocationName(out string deviceLocationName)
        {
            ResultCode result = ResultCode.UninitializedClock;

            deviceLocationName = null;

            lock (_lock)
            {
                if (_isInitialized)
                {
                    deviceLocationName = _deviceLocationName;
                    result             = ResultCode.Success;
                }
            }

            return result;
        }

        public ResultCode SetDeviceLocationNameWithTimeZoneRule(string locationName, Stream timeZoneBinaryStream)
        {
            ResultCode result = ResultCode.TimeZoneConversionFailed;

            lock (_lock)
            {
                bool timeZoneConversionSuccess = TimeZone.ParseTimeZoneBinary(out TimeZoneRule rules, timeZoneBinaryStream);

                if (timeZoneConversionSuccess)
                {
                    _deviceLocationName = locationName;
                    _myRules            = rules;
                    result              = ResultCode.Success;
                }
            }

            return result;
        }

        public void SetTotalLocationNameCount(uint totalLocationNameCount)
        {
            lock (_lock)
            {
                _totalLocationNameCount = totalLocationNameCount;
            }
        }

        public ResultCode GetTotalLocationNameCount(out uint totalLocationNameCount)
        {
            ResultCode result = ResultCode.UninitializedClock;

            totalLocationNameCount = 0;

            lock (_lock)
            {
                if (_isInitialized)
                {
                    totalLocationNameCount = _totalLocationNameCount;
                    result                 = ResultCode.Success;
                }
            }

            return result;
        }

        public ResultCode SetUpdatedTime(SteadyClockTimePoint timeZoneUpdatedTimePoint, bool bypassUninitialized = false)
        {
            ResultCode result = ResultCode.UninitializedClock;

            lock (_lock)
            {
                if (_isInitialized || bypassUninitialized)
                {
                    _timeZoneUpdateTimePoint = timeZoneUpdatedTimePoint;
                    result                   = ResultCode.Success;
                }
            }

            return result;
        }

        public ResultCode GetUpdatedTime(out SteadyClockTimePoint timeZoneUpdatedTimePoint)
        {
            ResultCode result;

            lock (_lock)
            {
                if (_isInitialized)
                {
                    timeZoneUpdatedTimePoint = _timeZoneUpdateTimePoint;
                    result                   = ResultCode.Success;
                }
                else
                {
                    timeZoneUpdatedTimePoint = SteadyClockTimePoint.GetRandom();
                    result                   = ResultCode.UninitializedClock;
                }
            }

            return result;
        }

        public ResultCode ParseTimeZoneRuleBinary(out TimeZoneRule outRules, Stream timeZoneBinaryStream)
        {
            ResultCode result = ResultCode.Success;

            lock (_lock)
            {
                bool timeZoneConversionSuccess = TimeZone.ParseTimeZoneBinary(out outRules, timeZoneBinaryStream);

                if (!timeZoneConversionSuccess)
                {
                    result = ResultCode.TimeZoneConversionFailed;
                }
            }

            return result;
        }

        public void SetTimeZoneRuleVersion(UInt128 timeZoneRuleVersion)
        {
            lock (_lock)
            {
                _timeZoneRuleVersion = timeZoneRuleVersion;
            }
        }

        public ResultCode GetTimeZoneRuleVersion(out UInt128 timeZoneRuleVersion)
        {
            ResultCode result;

            lock (_lock)
            {
                if (_isInitialized)
                {
                    timeZoneRuleVersion = _timeZoneRuleVersion;
                    result              = ResultCode.Success;
                }
                else
                {
                    timeZoneRuleVersion = new UInt128();
                    result              = ResultCode.UninitializedClock;
                }
            }

            return result;
        }

        public ResultCode ToCalendarTimeWithMyRules(long time, out CalendarInfo calendar)
        {
            ResultCode result;

            lock (_lock)
            {
                if (_isInitialized)
                {
                    result = ToCalendarTime(_myRules, time, out calendar);
                }
                else
                {
                    calendar = new CalendarInfo();
                    result   = ResultCode.UninitializedClock;
                }
            }

            return result;
        }

        public ResultCode ToCalendarTime(TimeZoneRule rules, long time, out CalendarInfo calendar)
        {
            ResultCode result;

            lock (_lock)
            {
                result = TimeZone.ToCalendarTime(rules, time, out calendar);
            }

            return result;
        }

        public ResultCode ToPosixTimeWithMyRules(CalendarTime calendarTime, out long posixTime)
        {
            ResultCode result;

            lock (_lock)
            {
                if (_isInitialized)
                {
                    result = ToPosixTime(_myRules, calendarTime, out posixTime);
                }
                else
                {
                    posixTime = 0;
                    result    = ResultCode.UninitializedClock;
                }
            }

            return result;
        }

        public ResultCode ToPosixTime(TimeZoneRule rules, CalendarTime calendarTime, out long posixTime)
        {
            ResultCode result;

            lock (_lock)
            {
                result = TimeZone.ToPosixTime(rules, calendarTime, out posixTime);
            }

            return result;
        }
    }
}
