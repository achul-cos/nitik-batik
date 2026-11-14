using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    [Header("UI Component")]
    public Button tombolAmbilPesanan;
    public Button tombolSelesaiPesanan;

    [Header("Antiran Customer Configuration")]
    public CustomerChair[] customerChairs;

    public List<CustomerSlot> customerObjects = new();

    public float kecepatanGerakCustomer = 3f;
    public float jedaAnimasiJalanCustomer = 0.3f;
    public float jedaGerakAntarCustomer = 1f;

    private void Start()
    {
        InitializeCustomerSlot();

        tombolAmbilPesanan.onClick.AddListener(OnAmbilPesanan);
        tombolSelesaiPesanan.onClick.AddListener(OnSelesaiPesanan);
        tombolAmbilPesanan.interactable = false;
    }

    private void Update()
    {
        RefreshAntrian();
    }

    private void OnEnable()
    {
        if (CustomerManager.Instance != null)
        {
            CustomerManager.Instance.CustomerArrived += HandleCustomerArrive;
            CustomerManager.Instance.CustomerServed += HandleCustomerServe;
        }
    }

    private void OnDisable()
    {
        if (CustomerManager.Instance != null)
        {
            CustomerManager.Instance.CustomerArrived -= HandleCustomerArrive;
            CustomerManager.Instance.CustomerServed -= HandleCustomerServe;
        }
    }

    private void HandleCustomerArrive(Customer customer)
    {
       IEnumerator EnablingAmbilCustomerButton()
        {
            yield return new WaitForSeconds(kecepatanGerakCustomer * 1.5f);
            tombolAmbilPesanan.interactable = true;
        }

        StartCoroutine(EnablingAmbilCustomerButton());
    }

    private void HandleCustomerServe(Customer customer)
    {
        IEnumerator CooldownAmbilCustomerButton()
        {
            tombolAmbilPesanan.interactable = false;
            yield return new WaitForSeconds(kecepatanGerakCustomer * 1.5f);
            tombolAmbilPesanan.interactable = true;
        }

        StartCoroutine(CooldownAmbilCustomerButton());
    }

    public void RefreshAntrian()
    {
        if (CustomerManager.Instance == null)
            return;

        CustomerSlot CreateCustomerSlot()
        {
            GameObject go = new GameObject("Customer");

            CustomerSlot slot = go.AddComponent<CustomerSlot>();

            slot.moveSpeed = kecepatanGerakCustomer;
            slot.animationSpeed = jedaAnimasiJalanCustomer;

            customerObjects.Add(slot);

            return slot;
        }

        List<Customer> queue = CustomerManager.Instance.CustomerQueue.ToList();

        // Snapshot slot yang sudah ada sebelum refresh
        List<CustomerSlot> availableSlots = new(customerObjects);

        //--------------------------------------------------
        // Customer yang ada di queue
        //--------------------------------------------------

        for (int i = 0; i < queue.Count; i++)
        {
            Customer customer = queue[i];
            CustomerChair chair = customerChairs[i];

            CustomerSlot slot = null;

            // Cari slot pertama yang cocok
            for (int j = 0; j < availableSlots.Count; j++)
            {
                if (availableSlots[j].Customer == customer)
                {
                    slot = availableSlots[j];
                    availableSlots.RemoveAt(j); // tandai sudah dipakai
                    break;
                }
            }

            // Belum ada object customer
            if (slot == null)
            {
                slot = CreateCustomerSlot();

                slot.SetCustomer(customer);

                slot.Enter(
                    chair.offPosition,
                    chair.originalPosition
                );
            }
            else
            {
                slot.Move(
                    chair.originalPosition,
                    delay: jedaGerakAntarCustomer * i + 1f
                );
            }
        }

        //--------------------------------------------------
        // Customer yang keluar
        //--------------------------------------------------

        List<CustomerSlot> removeSlots = new();

        foreach (CustomerSlot slot in availableSlots)
        {
            slot.Exit(customerChairs[^1].offPosition);

            removeSlots.Add(slot);
        }

        foreach (CustomerSlot slot in removeSlots)
        {
            customerObjects.Remove(slot);
        }
    }

    private void InitializeCustomerSlot()
    {
        for (int i = 0; i < customerChairs.Length; i++)
        {
            customerChairs[i].index = i;
        }
    }

    public void OnAmbilPesanan()
    {
        CustomerManager.Instance.GetNextCustomer();
    }

    public void OnSelesaiPesanan()
    {
        CustomerManager.Instance.FinishCurrentCustomer();
    }
}