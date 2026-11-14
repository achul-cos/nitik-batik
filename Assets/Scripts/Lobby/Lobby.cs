using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pengatur scene Lobby: menampilkan antrean customer di kursi, memilih customer, dan membuka dialog.
/// <para />
/// Pemain memilih customer dengan mengklik customer paling depan antrean (setelah ia duduk).
/// Tampilan antrean hanya diperbarui saat ada customer datang atau selesai dilayani (berbasis event).
/// </summary>
public class Lobby : MonoBehaviour
{
    [Header("UI Component")]
    public DialogPanel dialogPanel;

    [Header("Antiran Customer Configuration")]
    public CustomerChair[] customerChairs;

    public List<CustomerSlot> customerObjects = new();

    [Tooltip("Opsional. Jika kosong, objek customer dibuat lewat kode.")]
    public CustomerSlot prefabCustomerSlot;

    public float kecepatanGerakCustomer = 3f;
    public float jedaAnimasiJalanCustomer = 0.3f;
    public float jedaGerakAntarCustomer = 1f;

    private CustomerManager _customerManager;
    private GameManager _gameManager;

    private void Start()
    {
        InitializeCustomerSlot();

        _customerManager = CustomerManager.Instance;
        _gameManager = GameManager.Instance;

        if (_customerManager != null)
        {
            _customerManager.CustomerArrived += HandleCustomerArrive;
            _customerManager.CustomerServed += HandleCustomerServe;
        }

        if (_gameManager != null)
            _gameManager.OnDialogStarted += HandleDialogStarted;

        if (dialogPanel != null)
            dialogPanel.DialogSelesai += HandleDialogSelesai;

        // Jika pemain baru kembali dari menjemur, pesanannya selesai dan customernya pergi
        // (dilakukan sebelum antrean ditampilkan agar customer itu tidak sempat muncul lagi).
        if (_customerManager != null)
            _customerManager.LayaniPesananSelesai();

        RefreshAntrian();
    }

    private void OnDestroy()
    {
        if (_customerManager != null)
        {
            _customerManager.CustomerArrived -= HandleCustomerArrive;
            _customerManager.CustomerServed -= HandleCustomerServe;
        }

        if (_gameManager != null)
            _gameManager.OnDialogStarted -= HandleDialogStarted;

        if (dialogPanel != null)
            dialogPanel.DialogSelesai -= HandleDialogSelesai;

        foreach (CustomerSlot slot in customerObjects)
        {
            if (slot == null) continue;

            slot.Dipilih -= HandleCustomerDipilih;
            slot.SelesaiBergerak -= HandleSlotSelesaiBergerak;
        }
    }

    #region Event handler

    private void HandleCustomerArrive(Customer customer)
    {
        RefreshAntrian();
    }

    private void HandleCustomerServe(Customer customer)
    {
        RefreshAntrian();
    }

    private void HandleDialogStarted(Customer customer)
    {
        PerbaruiPilihan();

        if (dialogPanel != null)
            dialogPanel.Tampilkan(customer);
    }

    private void HandleDialogSelesai()
    {
        GameManager.Instance.NextFabric();
    }

    private void HandleSlotSelesaiBergerak(CustomerSlot slot)
    {
        PerbaruiPilihan();
    }

    /// <summary>
    /// Pemain mengklik customer. Hanya customer terdepan yang sudah duduk yang bisa sampai ke sini.
    /// </summary>
    private void HandleCustomerDipilih(CustomerSlot slot)
    {
        if (_gameManager == null || _gameManager.IsPaused) return;

        if (!BolehDipilih(slot)) return;

        // NextDialog mengambil customer terdepan dan memicu OnDialogStarted (yang membuka dialog).
        _gameManager.NextDialog();

        PerbaruiPilihan();
    }

    #endregion

    #region Memilih customer

    /// <summary>
    /// Customer boleh dipilih jika: paling depan antrean, sudah duduk, dan belum ada customer lain yang sedang dilayani.
    /// </summary>
    private bool BolehDipilih(CustomerSlot slot)
    {
        if (slot == null || _customerManager == null) return false;

        if (slot.isMoving) return false;

        if (_customerManager.CustomerCurrent != null) return false;

        return _customerManager.PeekCustomerAtIndex(0, isDebug: false) == slot.Customer;
    }

    private void PerbaruiPilihan()
    {
        foreach (CustomerSlot slot in customerObjects)
        {
            if (slot != null) slot.SetBisaDipilih(BolehDipilih(slot));
        }
    }

    #endregion

    #region Antrean

    /// <summary>
    /// Menyamakan objek customer di scene dengan antrean di CustomerManager.
    /// </summary>
    public void RefreshAntrian()
    {
        if (_customerManager == null || customerChairs == null || customerChairs.Length == 0)
            return;

        // Slot yang belum dicocokkan dengan customer di antrean; sisanya akan keluar.
        List<CustomerSlot> slotTersedia = new(customerObjects);

        int index = 0;

        foreach (Customer customer in _customerManager.CustomerQueue)
        {
            // Customer di luar jumlah kursi belum ditampilkan.
            if (index >= customerChairs.Length) break;

            CustomerChair chair = customerChairs[index];

            CustomerSlot slot = CariSlot(slotTersedia, customer);

            if (slot == null)
            {
                slot = BuatCustomerSlot();

                slot.SetCustomer(customer);

                slot.Enter(chair.offPosition, chair.originalPosition);
            }
            else
            {
                slot.Move(chair.originalPosition, delay: jedaGerakAntarCustomer * index + 1f);
            }

            index++;
        }

        // Customer yang sudah tidak ada di antrean berjalan keluar.
        foreach (CustomerSlot slot in slotTersedia)
        {
            slot.Dipilih -= HandleCustomerDipilih;
            slot.SelesaiBergerak -= HandleSlotSelesaiBergerak;

            slot.Exit(customerChairs[^1].offPosition);

            customerObjects.Remove(slot);
        }

        PerbaruiPilihan();
    }

    private static CustomerSlot CariSlot(List<CustomerSlot> slotTersedia, Customer customer)
    {
        for (int i = 0; i < slotTersedia.Count; i++)
        {
            if (slotTersedia[i].Customer == customer)
            {
                CustomerSlot slot = slotTersedia[i];
                slotTersedia.RemoveAt(i);
                return slot;
            }
        }

        return null;
    }

    private CustomerSlot BuatCustomerSlot()
    {
        CustomerSlot slot;

        if (prefabCustomerSlot != null)
        {
            slot = Instantiate(prefabCustomerSlot);
            slot.name = "Customer";
        }
        else
        {
            slot = new GameObject("Customer").AddComponent<CustomerSlot>();
        }

        slot.moveSpeed = kecepatanGerakCustomer;
        slot.animationSpeed = jedaAnimasiJalanCustomer;

        slot.Dipilih += HandleCustomerDipilih;
        slot.SelesaiBergerak += HandleSlotSelesaiBergerak;

        customerObjects.Add(slot);

        return slot;
    }

    private void InitializeCustomerSlot()
    {
        for (int i = 0; i < customerChairs.Length; i++)
        {
            customerChairs[i].index = i;
        }
    }

    #endregion
}
