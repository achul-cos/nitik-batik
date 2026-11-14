using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pemilih acak berbobot: item dengan bobot lebih besar lebih mungkin terpilih.
/// Bobot tidak harus berjumlah 1 (dinormalisasi otomatis).
/// </summary>
public static class PengacakBerbobot
{
    /// <summary>
    /// Memilih satu item secara acak sesuai bobotnya. Bobot nol atau negatif tidak pernah terpilih.
    /// Jika semua bobot nol, item dipilih seragam (merata).
    /// </summary>
    /// <param name="items">Daftar item yang dipilih</param>
    /// <param name="bobot">Bobot tiap item, urutannya sama dengan <paramref name="items"/></param>
    public static T Pilih<T>(IReadOnlyList<T> items, IReadOnlyList<float> bobot)
    {
        if (items == null || items.Count == 0)
            return default;

        float total = 0f;

        for (int i = 0; i < items.Count; i++)
        {
            total += Mathf.Max(0f, i < bobot.Count ? bobot[i] : 0f);
        }

        if (total <= 0f)
            return items[Random.Range(0, items.Count)];

        float titik = Random.value * total;

        for (int i = 0; i < items.Count; i++)
        {
            float bobotItem = Mathf.Max(0f, i < bobot.Count ? bobot[i] : 0f);

            if (bobotItem <= 0f) continue;

            if (titik < bobotItem)
                return items[i];

            titik -= bobotItem;
        }

        // Cadangan untuk selisih pembulatan float: ambil item terakhir yang berbobot.
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (i < bobot.Count && bobot[i] > 0f)
                return items[i];
        }

        return items[items.Count - 1];
    }
}
