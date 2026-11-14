using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pengatur scene Lobby: menampilkan antrean customer di kursi, tombol ambil/selesai pesanan, dan dialog.
/// Tampilan antrean hanya diperbarui saat ada customer datang atau selesai dilayani (berbasis event).
/// </summary>
public class Lobby : MonoBehaviour
{
    [Header("UI Component")]
    public Button tombolAmbilPesanan;
    public Button tombolSelesaiPesanan;
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
    private Coroutine _jedaTombolCoroutine;
    private bool _menungguGerakCustomer;

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

        tombolAmbilPesanan.onClick.AddListener(OnAmbilPesanan);
        tombolSelesaiPesanan.onClick.AddListener(OnSelesaiPesanan);

        RefreshAntrian();
        MulaiJedaTombol();
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
    }

    #region Event handler

    private void HandleCustomerArrive(Customer customer)
    {
        RefreshAntrian();
        MulaiJedaTombol();
    }

    private void HandleCustomerServe(Customer customer)
    {
        RefreshAntrian();
        MulaiJedaTombol();
    }

    private void HandleDialogStarted(Customer customer)
    {
        PerbaruiTombolAmbil();

        if (dialogPanel != null)
            dialogPanel.Tampilkan(customer);
    }

    private void HandleDialogSelesai()
    {
        GameManager.Instance.NextFabric();
    }

    #endregion

    #region Tombol

    /// <summary>
    /// Tombol ambil pesanan baru aktif setelah customer selesai berjalan ke kursi.
    /// Jika ada jeda yang sedang berjalan, jeda diulang (tidak menumpuk).
    /// </summary>
    private void MulaiJedaTombol()
    {
        if (_jedaTombolCoroutine != null) StopCoroutine(_jedaTombolCoroutine);

        _jedaTombolCoroutine = StartCoroutine(JedaTombol());
    }

    private IEnumerator JedaTombol()
    {
        _menungguGerakCustomer = true;
        PerbaruiTombolAmbil();

        yield return new WaitForSeconds(kecepatanGerakCustomer * 1.5f);

        _menungguGerakCustomer = false;
        _jedaTombolCoroutine = null;
        PerbaruiTombolAmbil();
    }

    private void PerbaruiTombolAmbil()
    {
        if (tombolAmbilPesanan == null || _customerManager == null) return;

        tombolAmbilPesanan.interactable =
            !_menungguGerakCustomer &&
            _customerManager.CustomerCurrent == null &&
            _customerManager.CustomerQueueCount > 0;
    }

    public void OnAmbilPesanan()
    {
        // NextDialog mengambil customer terdepan dan memicu OnDialogStarted (yang membuka dialog).
        GameManager.Instance.NextDialog();
        PerbaruiTombolAmbil();
    }

    public void OnSelesaiPesanan()
    {
        CustomerManager.Instance.FinishCurrentCustomer();
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
            slot.Exit(customerChairs[^1].offPosition);

            customerObjects.Remove(slot);
        }
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
