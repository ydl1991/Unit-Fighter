using System;

public class XOrShiftRNG
{
    ulong m_seed1;
    ulong m_seed2;

    // initializing seed with GUID
    public XOrShiftRNG()
    {
        // Read an interesting article about xorshift saying about generating random guid as seed,
        // not very good performance-wise but can produce very good seeds. Wanna try it here.
        m_seed1 = (ulong)Guid.NewGuid().GetHashCode();
        m_seed2 = (ulong)Guid.NewGuid().GetHashCode();
    }

    // manially seeding
    public XOrShiftRNG(ulong seed)
    {
        m_seed1 = seed << 3 | seed;
        m_seed2 = seed >> 7 | seed;
    }

    // Random bool
    public bool RandomBool()
    {
        if (RandomIntRange(0, 100) >= 50)
            return true;
        
        return false;
    }

    // Generating random 32 bit integer
    public int RandomInt()
    {
        int result;

        // mangling...
        ulong temp1 = m_seed2;
        m_seed1 ^= m_seed1 << 23;
        ulong temp2 = m_seed1 ^ m_seed2 ^ (m_seed1 >> 17) ^ (m_seed2 >> 26);
        result = (int)(temp2 + m_seed2);

        // reassign seed
        m_seed1 = temp1;
        m_seed2 = temp2;

        return result;
    }

    // Generate random int in range ( min, max ]
    public int RandomIntRange(int min, int max)
    {
        float randomFloat = RandomFloat();
        int range = max - min;
        return (int)(randomFloat * range) + min;
    }

    // Generating random float from 0 to 1
    public float RandomFloat()
    {
        float result;

        // mangling...
        ulong temp1 = m_seed2;
        m_seed1 ^= m_seed1 << 23;
        ulong temp2 = m_seed1 ^ m_seed2 ^ (m_seed1 >> 17) ^ (m_seed2 >> 26);
        ulong temp3 = temp2 + m_seed2;

        result = 1.0f / (int.MaxValue + 1.0f) * (0x7FFFFFFF & temp3);

        // reassign seed
        m_seed1 = temp1;
        m_seed2 = temp2;

        return result;       
    }

    // Generate random float in range ( min, max ]
    public float RandomFloatRange(float min, float max)
    {
        float randomFloat = RandomFloat();
        float range = max - min;
        return randomFloat * range + min;
    }

    // generate a high precision decimal between 0 and 1
    public decimal RandomDecimal()
    {
        // mangling...
        ulong temp1 = m_seed2;
        m_seed1 ^= m_seed1 << 23;
        ulong temp2 = m_seed1 ^ m_seed2 ^ (m_seed1 >> 17) ^ (m_seed2 >> 26);
        ulong temp3 = temp2 + m_seed2;

        int high = (int)(temp3 & 0x1FFFFFFF);
        int mid = (int)(temp3 >> 16);
        int low = (int)(temp3 >> 32);
        decimal dec = new decimal(low, mid, high, false, 28);

        // reassign seed
        m_seed1 = temp1;
        m_seed2 = temp2;

        return dec;
    }
}
