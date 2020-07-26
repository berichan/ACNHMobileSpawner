package com.berichan.usbunitylibrary;

import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbEndpoint;
import android.hardware.usb.UsbInterface;
import android.hardware.usb.UsbManager;
import android.util.Log;
import android.widget.Toast;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.time.LocalTime;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Hashtable;

public class USBUtil {

    UsbManager usbManager;
    UsbDevice usbDevice;

    Context unityContext;

    Hashtable<Integer, Long> lastToastTimes;

    USBUtil() {}

    public void CreateToast(Context ctx, String message)
    {
        Log.d("Unity", "create toast called");
        boolean longDuration = true;
        Toast.makeText(ctx, message, longDuration ? Toast.LENGTH_LONG : Toast.LENGTH_SHORT).show();
    }

    private void createToastInternal(Context ctx, String message, Integer functionId)
    {
        // internal toasts have a cooldown of 700 ms
        if (lastToastTimes == null)
            lastToastTimes = new Hashtable<Integer, Long>();

        if (lastToastTimes.containsKey(functionId))
        {
            Long currTime = System.currentTimeMillis();
            Long lastTime = lastToastTimes.get(functionId);
            if (currTime - lastTime > 700L)
            {
                CreateToast(ctx, message);
                lastToastTimes.put(functionId, currTime);
            }
        }
        else
        {
            Long currTime = System.currentTimeMillis();
            CreateToast(ctx, message);
            lastToastTimes.put(functionId, currTime);
        }
    }

    public void Init(Context ctx)
    {
        unityContext = ctx;

    }

    public boolean ConnectUSB()
    {
        usbManager = (UsbManager) unityContext.getSystemService(Context.USB_SERVICE);
        if(usbManager == null)
        {
            Log.d("Unity", "No usb manager");
            return false;
        }
        HashMap<String, UsbDevice> deviceHashMap;
        deviceHashMap = usbManager.getDeviceList();
        for (UsbDevice device : deviceHashMap.values()) {
            if (device.getVendorId() == 1406 && device.getProductId() == 12288) {
                usbDevice = device;
            }
        }
        if(usbDevice == null)
        {
            Log.d("Unity", "No usb device");
            createToastInternal(unityContext, "no usb device", 1);
            return false;
        }
        else
        {
            createToastInternal(unityContext, "usb found:" + usbDevice.getDeviceName(), 2);
        }

        if (! usbManager.hasPermission(usbDevice)){
            usbManager.requestPermission(usbDevice, PendingIntent.getBroadcast(unityContext, 0, new Intent("com.berichan.usbunitylibrary.USB_PERMISSION"), 0));
        }

        return true;
    }

    public void SendData(byte[] unsignedByteCount, byte[] bytes)
    {
        int bytesWritten;
        if (usbDevice == null)
        {
            createToastInternal(unityContext, "no usb device", 3);
            return;
        }
        UsbInterface intf = usbDevice.getInterface(0);
        UsbEndpoint endpoint = intf.getEndpoint(1);
        UsbDeviceConnection connection = usbManager.openDevice(usbDevice);

        if (connection == null)
        {
            createToastInternal(unityContext, "connection failed", 4);
            return;
        }

        connection.claimInterface(intf, true);
        bytesWritten = connection.bulkTransfer(endpoint, unsignedByteCount, unsignedByteCount.length, 5050);
        if (bytesWritten == -1)
            createToastInternal(unityContext, "USB-Botbase transfer Error: This is usually because your USB OTG adaptor is non-existent, not functioning or installed incorrectly, or your console requires a restart.", 5);
        bytesWritten = connection.bulkTransfer(endpoint, bytes, bytes.length, 5050);

        connection.releaseInterface(intf);
        connection.close();
    }

    public byte[] ReadData(int bytesExpected)
    {
        byte[] countReadBuffer = new byte[4];
        byte[] readBuffer = new byte[bytesExpected];
        int readResult;

        if (usbDevice == null)
        {
            createToastInternal(unityContext, "no usb device", 6);
            return null;
        }
        UsbInterface intf = usbDevice.getInterface(0);
        UsbEndpoint endpoint = intf.getEndpoint(0);
        UsbDeviceConnection connection = usbManager.openDevice(usbDevice);

        if (connection == null)
        {
            createToastInternal(unityContext, "connection failed", 7);
            return null;
        }

        connection.claimInterface(intf, true);
        readResult = connection.bulkTransfer(endpoint, countReadBuffer, 4, 1000);
        if (readResult == -1)
            createToastInternal(unityContext, "USB-Botbase transfer Error: This is usually because your USB OTG adaptor is non-existent, not functioning or installed incorrectly, or your console requires a restart.", 8);
        readResult = connection.bulkTransfer(endpoint, readBuffer, bytesExpected, 1000);

        connection.releaseInterface(intf);
        connection.close();

        return Arrays.copyOf(readBuffer, readResult);
    }

    public String getMacAddress(String ipAddress) {
        try {
            BufferedReader br = new BufferedReader(new FileReader(new File("/proc/net/arp")));
            String line;
            while((line = br.readLine()) != null) {
                if(line.contains(ipAddress)) {
                    /* this string still would need to be sanitized */
                    return line;
                }
            }
        } catch (IOException e) {
            e.printStackTrace();
            return "Error:" + e.getMessage();
        }
        return null;
    }

}
