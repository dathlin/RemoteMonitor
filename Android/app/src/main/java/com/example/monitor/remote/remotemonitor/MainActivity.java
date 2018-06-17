package com.example.monitor.remote.remotemonitor;

import android.graphics.Color;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import com.jjoe64.graphview.series.DataPoint;
import com.jjoe64.graphview.series.LineGraphSeries;

import org.json.JSONObject;


import HslCommunication.BasicFramework.SoftBasic;
import HslCommunication.Core.Net.NetHandle;
import HslCommunication.Core.Types.ActionOperateExTwo;
import HslCommunication.Core.Types.OperateResult;
import HslCommunication.Core.Types.OperateResultExOne;
import HslCommunication.Enthernet.PushNet.NetPushClient;
import HslCommunication.Enthernet.SimplifyNet.NetSimplifyClient;
import HslCommunication.Utilities;

public class MainActivity extends AppCompatActivity {

    private NetPushClient netPushClient = new NetPushClient("192.168.1.110", 23467, "A");
    private NetSimplifyClient netSimplifyClient = new NetSimplifyClient("192.168.1.110", 23457);
    private float[] dataArray = new float[80];
    private MyHandler myHandler;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        myHandler = new MyHandler();


        new Thread() {
            @Override
            public void run() {
                OperateResult create = netPushClient.CreatePush(new ActionOperateExTwo<NetPushClient, String>() {
                    @Override
                    public void Action(NetPushClient content1, String content2) {
                        try {
                            Message msg = new Message();
                            msg.what = 1;
                            // Bundle是message中的数据
                            Bundle b = new Bundle();
                            b.putString("data", content2);
                            msg.setData(b);
                            // 传递数据
                            myHandler.sendMessage(msg); // 向Handler发送消息,更新UI
                        } catch (Exception ex) {
                            Log.d("hsl", ex.getMessage());
                        }
                    }
                });
                if (create.IsSuccess) {
                    Log.d("hsl", "connect success");
                } else {
                    Log.d("hsl", "connect" + create.Message);
                    try {
                        Log.d("hsl", SoftBasic.ByteToHexString(Utilities.string2Byte("A"), '-'));
                    } catch (Exception ex) {

                    }
                }
            }
        }.start();

        Button buttonstart = (Button) findViewById(R.id.button_start);
        buttonstart.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                new Thread() {
                    @Override
                    public void run() {
                        OperateResultExOne<String> start = netSimplifyClient.ReadFromServer(new NetHandle(1), "");

                        Message msg = new Message() ;
                        msg.what = 2;
                        String content = start.IsSuccess?start.Content:"启动失败：" + start.Message;
                        Bundle b = new Bundle();
                        b.putString("data", content);
                        msg.setData(b);
                        myHandler.sendMessage(msg);
                    }
                }.start();
            }
        });

        Button buttonstop = (Button) findViewById(R.id.button_stop);
        buttonstop.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                new Thread() {
                    @Override
                    public void run() {
                        OperateResultExOne<String> stop = netSimplifyClient.ReadFromServer(new NetHandle(2), "");

                        Message msg = new Message();
                        msg.what = 2;
                        String content = stop.IsSuccess ? stop.Content : "停止失败：" + stop.Message;
                        Bundle b = new Bundle();
                        b.putString("data", content);
                        msg.setData(b);
                        myHandler.sendMessage(msg);
                    }
                }.start();
            }
        });
    }

    @Override
    protected void onStart() {
        super.onStart();

    }

    @Override
    protected void onStop() {
        super.onStop();
        netPushClient.ClosePush();
    }

    class MyHandler extends Handler {

        public MyHandler() {
        }

        public MyHandler(Looper L) {
            super(L);
        }

        // 必须重写这个方法，用于处理message
        @Override
        public void handleMessage(Message msg) {
            // 这里用于更新UI
            switch (msg.what) {
                case 1: {
                    try {
                        JSONObject jsonObject = new JSONObject(msg.getData().getString("data"));
                        TextView textViewStatus = (TextView) findViewById(R.id.textView2);
                        textViewStatus.setText(jsonObject.getBoolean("enable") ? "运行中" : "未运行");

                        TextView textViewTemp = (TextView) findViewById(R.id.textView4);
                        double value = jsonObject.getDouble("temp");
                        textViewTemp.setText(String.valueOf(value));
                        if (value > 100) {
                            textViewTemp.setBackgroundColor(Color.RED);
                        } else {
                            textViewTemp.setBackgroundColor(Color.WHITE);
                        }


                        TextView textViewProduct = (TextView) findViewById(R.id.textView6);
                        int product = jsonObject.getInt("product");
                        textViewProduct.setText(String.valueOf(product));


                        for(int i=0;i<dataArray.length-1;i++){
                            dataArray[i] = dataArray[i+1];
                        }
                        dataArray[dataArray.length - 1]=(float) value;
                        DataPoint[] dataPoints = new DataPoint[80];
                        for(int i=0;i<dataArray.length;i++){
                            dataPoints[i] = new DataPoint(i,dataArray[i]);
                        }

                        com.jjoe64.graphview.GraphView graph = (com.jjoe64.graphview.GraphView) findViewById(R.id.graph);
                        LineGraphSeries<DataPoint> series = new LineGraphSeries<DataPoint>(dataPoints);
                        graph.removeAllSeries();
                        graph.addSeries(series);



                        DashboardView dashboardView = (DashboardView) findViewById(R.id.dashboard_view);
                        dashboardView.setRealTimeValue((float) value);

                    } catch (Exception ex) {
                        Log.d("hsl", ex.getMessage());
                    }
                    break;
                }
                case 2:{
                    Toast.makeText(MainActivity.this, msg.getData().getString("data"), Toast.LENGTH_LONG).show();
                    break;
                }
            }

        }

    }
}
