using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Scriptable Object dari CustomerDatabase yang menyimpan data-data customer
/// </summary>
[CreateAssetMenu(fileName = "CustomersDatabase", menuName = "AksaraBatik/CustomersDatabase")]
public class CustomersDatabase : ScriptableObject
{
    private static readonly WealthType[] SemuaWealth = (WealthType[])Enum.GetValues(typeof(WealthType));
    private static readonly PerfectType[] SemuaPerfect = (PerfectType[])Enum.GetValues(typeof(PerfectType));

    /// <summary>
    /// List customer yang didaftarkan
    /// </summary>
    [Expandable] public List<Customer> customers = new List<Customer>();

    /// <summary>
    /// Mengambil data customer berdasarkan ID nya
    /// </summary>
    /// <param name="id">Id Customer</param>
    /// <returns></returns>
    public Customer GetCustomerById(string id)
    {
        foreach (Customer customer in customers)
        {
            if (customer != null && customer.customerId == id)
            {
                return customer;
            }
        }

        Debug.LogError($"CustomersDatabase [GetCustomerById] Error : Tidak ditemukan customer dengan Id {id}");
        return null;
    }

    /// <summary>
    /// "Mengacak" satu customer berdasarkan peluang kekayaan dan perfeksionisme pada <paramref name="dayConfig"/>.
    /// Tipe kekayaan dan perfeksionis diundi berbobot, lalu dipilih customer yang cocok dengan kedua tipe itu.
    /// Jika tidak ada customer yang cocok, undian diulang hingga <paramref name="percobaan"/> kali, lalu
    /// memakai tipe default (BIASA, C), dan terakhir customer mana pun yang tersedia.
    /// </summary>
    /// <returns>Customer yang terpilih, atau null jika database kosong.</returns>
    public Customer AmbilCustomerAcak(DayConfig dayConfig, int percobaan = 5)
    {
        List<Customer> kandidatSemua = customers.Where(c => c != null).ToList();

        if (kandidatSemua.Count == 0)
        {
            Debug.LogError("CustomersDatabase [AmbilCustomerAcak] : Database tidak punya customer.");
            return null;
        }

        float[] bobotWealth = SemuaWealth.Select(dayConfig.GetWealthChance).ToArray();
        float[] bobotPerfect = SemuaPerfect.Select(dayConfig.GetPerfectChance).ToArray();

        for (int i = 0; i < percobaan; i++)
        {
            WealthType wealth = PengacakBerbobot.Pilih(SemuaWealth, bobotWealth);
            PerfectType perfect = PengacakBerbobot.Pilih(SemuaPerfect, bobotPerfect);

            Customer terpilih = AcakDariTipe(kandidatSemua, wealth, perfect);

            if (terpilih != null) return terpilih;
        }

        Customer cadangan = AcakDariTipe(kandidatSemua, WealthType.BIASA, PerfectType.C);

        if (cadangan != null) return cadangan;

        Debug.LogWarning("CustomersDatabase [AmbilCustomerAcak] : Tidak ada customer yang cocok dengan peluang hari ini, memakai customer acak.");

        return kandidatSemua[Random.Range(0, kandidatSemua.Count)];
    }

    private static Customer AcakDariTipe(List<Customer> daftar, WealthType wealth, PerfectType perfect)
    {
        List<Customer> cocok = daftar.Where(c => c.customerWealth == wealth && c.customerPerfect == perfect).ToList();

        return cocok.Count == 0 ? null : cocok[Random.Range(0, cocok.Count)];
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (customers.Count <= 0) return;

        for (int i = 0; i < customers.Count; i++)
        {
            if (customers[i] == null) continue;

            string idBaru = (i + 1).ToString();

            if (customers[i].customerId == idBaru) continue;

            customers[i].customerId = idBaru;
            UnityEditor.EditorUtility.SetDirty(customers[i]);
        }
    }
#endif
}
