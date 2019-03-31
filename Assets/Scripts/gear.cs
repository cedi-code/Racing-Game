using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    class gear
    {
        public gear(float minKMH, float minRPM, float maxKMH, float maxRPM)
        {
            _minRPM = minRPM;
            _minKMH = minKMH;
            _maxRPM = maxRPM;
            _maxKMH = maxKMH;
        }
        private float _minRPM;
        private float _minKMH;
        private float _maxRPM;
        private float _maxKMH;

        public bool speedFits(float kmh)
        {
            return kmh >= _minKMH && kmh <= _maxKMH;
        }

        public float interpolate(float kmh)
        {
            if ((_maxKMH - _minKMH) == 0)
            {
                return (_minRPM + _maxRPM) / 2;
            }
            return _minRPM + (kmh - _minKMH) * (_maxRPM - _minRPM) / (_maxKMH - _minKMH);
        }
    }
}
