using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6
{
    public class ConfigurationSetting
    {
        double _cacheTimeout = 5;

        string _settingValue, _settingName;
        DateTime _lastRetrieval;

        public ConfigurationSetting( string name, string value )
        {
            _settingName = name;
            _settingValue = value;
            _lastRetrieval = DateTime.Now;
        }

        public bool isValid( )
        {
            try
            {
                TimeSpan difference = DateTime.Now - _lastRetrieval;
                if ( difference.TotalMinutes > _cacheTimeout )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex , System.Reflection.MethodInfo.GetCurrentMethod());
            }
            return false;
        }

        public string Value
        {
            get
            {
                return _settingValue;
            }
            set
            {
                _settingValue = value;
                _lastRetrieval = DateTime.Now;
            }
        }

        public string Name
        {
            get
            {
                return _settingName;
            }
        }

    }
}