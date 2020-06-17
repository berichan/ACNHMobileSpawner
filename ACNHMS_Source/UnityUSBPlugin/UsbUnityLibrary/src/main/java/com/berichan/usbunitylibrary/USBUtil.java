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

import java.util.Arrays;
import java.util.HashMap;

public class USBUtil {

    UsbManager usbManager;
    UsbDevice usbDevice;

    Context unityContext;

    USBUtil() {}

    public void CreateToast(Context ctx, String message)
    {
        Log.d("Unity", "create toast called");
        boolean longDuration = true;
        Toast.makeText(ctx, message, longDuration ? Toast.LENGTH_LONG : Toast.LENGTH_SHORT).show();
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
            CreateToast(unityContext, "no usb device");
            return false;
        }
        else
        {
            CreateToast(unityContext, "usb found:" + usbDevice.getDeviceName());
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
            CreateToast(unityContext, "no usb device");
            return;
        }
        UsbInterface intf = usbDevice.getInterface(0);
        UsbEndpoint endpoint = intf.getEndpoint(1);
        UsbDeviceConnection connection = usbManager.openDevice(usbDevice);

        if (connection == null)
        {
            CreateToast(unityContext, "connection failed");
            return;
        }

        connection.claimInterface(intf, true);
        bytesWritten = connection.bulkTransfer(endpoint, unsignedByteCount, unsignedByteCount.length, 5050);
        CreateToast(unityContext, "Bytes written: " + bytesWritten);
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
            CreateToast(unityContext, "no usb device");
            return null;
        }
        UsbInterface intf = usbDevice.getInterface(0);
        UsbEndpoint endpoint = intf.getEndpoint(0);
        UsbDeviceConnection connection = usbManager.openDevice(usbDevice);

        if (connection == null)
        {
            CreateToast(unityContext, "connection failed");
            return null;
        }

        connection.claimInterface(intf, true);
        readResult = connection.bulkTransfer(endpoint, countReadBuffer, 4, 1000);
        CreateToast(unityContext, "Bytes read: " + readResult);
        readResult = connection.bulkTransfer(endpoint, readBuffer, bytesExpected, 1000);

        connection.releaseInterface(intf);
        connection.close();

        return Arrays.copyOf(readBuffer, readResult);
    }

}
