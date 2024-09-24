using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace FFmpeg.Unity
{
    public class FFUnityStream : Stream
    {
        public static HttpClient client = new HttpClient();//YouTube.Default.MakeClient();
        public MemoryStream ms = new MemoryStream(4096 * 4096);
        public long ReadPosition = 0;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length
        {
            get
            {
                // mutex.WaitOne();
                var ret = ms.Length;
                // mutex.ReleaseMutex();
                return ret;
            }
        }
        public override long Position
        {
            get
            {
                return ReadPosition;
                mutex.WaitOne();
                var ret = ms.Position;
                mutex.ReleaseMutex();
                return ret;
            }
            set
            {
                ReadPosition = value;
                return;
                mutex.WaitOne();
                // ms.Position = value;
                mutex.ReleaseMutex();
            }
        }
        public Thread thread;
        public Mutex mutex = new Mutex(false);
        public Action onBufferEmpty = null;
        private Queue<string> urls = new Queue<string>();
        private int errors = 0;

        public void StartLoop(string url, bool stream)
        {
            Reset();
            thread?.Abort();
            thread = new Thread(() =>
            {
                try
                {
                    LoopThread(url, stream);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            });
            thread.Name = "Stream Buffer Thread";
            thread.Start();
        }

        public void PushUrl(params string[] links)
        {
            if (mutex.WaitOne())
            {
                foreach (var url in links)
                    urls.Enqueue(url);
                mutex.ReleaseMutex();
            }
        }

        private async void LoopThread(string url, bool stream)
        {
            var request = new HttpRequestMessage(stream ? HttpMethod.Post : HttpMethod.Get, url);
            var response = (await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead));
            Debug.Log($"code: {response.StatusCode}");
            var content = response.Content;
            if (content == null)
            {
                Debug.LogError("Content == null");
            }
            Stream data = null;
            byte[] buffer = new byte[4096 * 4096];
            // if (!stream)
            data = await content.ReadAsStreamAsync();
            // int c = await data.ReadAsync(buffer);
            // Debug.Log($"out.dat {c}");
            // File.WriteAllBytes("out.dat", buffer);
            Debug.Log("begin");
            while (true)
            {
                int count = await data.ReadAsync(buffer);
                if (count <= 0)
                {
                    if (urls.Count == 0)
                    {
                        onBufferEmpty?.Invoke();
                        Thread.Yield();
                        continue;
                    }
                    if (mutex.WaitOne())
                    {
                        url = urls.Dequeue();
                        mutex.ReleaseMutex();
                    }
                    // break;
                    // Thread.Sleep(10);
                    try
                    {
                        request = new HttpRequestMessage(stream ? HttpMethod.Post : HttpMethod.Get, url);
                        content = (await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)).Content;
                        data = await content.ReadAsStreamAsync();
                    }
                    catch (WebException e)
                    {
                        Debug.LogException(e);
                    }
                    continue;
                }
                errors = 0;
                if (mutex.WaitOne())
                {
                    var pos = ms.Position;
                    ms.Seek(0, SeekOrigin.End);
                    await ms.WriteAsync(buffer, 0, count);
                    ms.Position = pos;
                    mutex.ReleaseMutex();
                }
                // Thread.Sleep(10);
                Thread.Yield();
            }
            while (false)
            {
                if (stream && false)
                {
                    var bytes = await content.ReadAsByteArrayAsync();
                    // lock (locker)
                    mutex.WaitOne();
                    {
                        var pos = ms.Position;
                        ms.Seek(0, SeekOrigin.End);
                        await ms.WriteAsync(bytes);
                        ms.Position = pos;
                        mutex.ReleaseMutex();
                    }
                    Thread.Sleep(750);
                    // request = new HttpRequestMessage(HttpMethod.Post, url);
                    request = new HttpRequestMessage(stream ? HttpMethod.Post : HttpMethod.Get, url);
                    content = (await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)).Content;
                }
                else
                {
                    // data.ReadTimeout = 2000;
                    try
                    {
                        // var stw = Stopwatch.StartNew();
                        int count = await data.ReadAsync(buffer);
                        // Debug.Log(count);
                        // stw.Stop();
                        if (count <= 0)
                        {
                            if (stream)
                            {
                                Thread.Sleep(500);
                                // request = new HttpRequestMessage(HttpMethod.Post, url);
                                // request = new HttpRequestMessage(stream ? HttpMethod.Post : HttpMethod.Get, url);
                                content = (await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)).Content;
                                data = await content.ReadAsStreamAsync();
                            }
                            else
                            {
                                if (errors++ > 100)
                                    break;
                                Thread.Sleep(100);
                            }
                        }
                        errors = 0;
                        // lock (locker)
                        if (mutex.WaitOne())
                        {
                            var pos = ms.Position;
                            ms.Seek(0, SeekOrigin.End);
                            await ms.WriteAsync(buffer, 0, count);
                            ms.Position = pos;
                            mutex.ReleaseMutex();
                        }
                    }
                    catch (WebException e)
                    {
                        Debug.LogException(e);
                    }
                    Thread.Sleep(10);
                    Thread.Yield();
                }
            }
            Debug.Log("Exiting Stream Buffer Thread");
        }

        public void Reset()
        {
            // lock (locker)
            if (mutex.WaitOne())
            {
                ms.Position = 0;
                ReadPosition = 0;
                errors = 0;
                mutex.ReleaseMutex();
            }
        }

        public override void Flush()
        {
            // lock (locker)
            if (mutex.WaitOne())
            {
                ms.Flush();
                mutex.ReleaseMutex();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // lock (locker)
            // mutex.WaitOne();
            if (mutex.WaitOne())
            {
                // if (Position == Length)
                {
                    // ReadPosition = 0;
                    // Debug.Log("NOOO~!");
                    // ms.Position = 0;
                }
                ms.Position = ReadPosition;
                var ret = ms.Read(buffer, offset, count);
                ReadPosition += ret;
                // Debug.Log(count);
                int j = 0;
                if (ret <= 0 && errors > 100)
                {
                    mutex.ReleaseMutex();
                    Debug.LogError("Done reading :(");
                    return ret;
                }
                while (ret <= 0 && j < 10 && errors <= 100)
                {
                    j++;
                    mutex.ReleaseMutex();
                    Debug.LogWarning("Is the buffer being read too fast?");
                    Thread.Sleep(10);
                    mutex.WaitOne();
                    ret = ms.Read(buffer, offset, count);
                    // return count;
                    // Debug.Log(Position);
                    // Debug.Log(Length);
                }
                // Debug.Log(ret);
                mutex.ReleaseMutex();
                return ret;
            }
            throw new Exception();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // lock (locker)
            mutex.WaitOne();
            {
                var ret = ms.Seek(offset, origin);
                mutex.ReleaseMutex();
                return ret;
            }
        }

        public override void SetLength(long value)
        {
            // lock (locker)
            mutex.WaitOne();
            {
                ms.SetLength(value);
                mutex.ReleaseMutex();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // lock (locker)
            mutex.WaitOne();
            {
                ms.Write(buffer, offset, count);
                mutex.ReleaseMutex();
            }
        }

        protected override void Dispose(bool disposing)
        {
            Debug.Log("Dispose of CustomStream");
            thread?.Abort();
            // mutex.WaitOne();
            ms.Dispose();
            // mutex.ReleaseMutex();
            base.Dispose(disposing);
        }
    }
}
