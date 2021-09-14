# prnt-sc-crawler
This Program generates random URLs for [prnt.sc](https://prnt.sc) and downloads the resulting images. Be careful when specifying many Threads or a small Wait-Intervall, Cloudflare might ban your IP.
## Parameters
|Name|Meaning|Example|
|---|---|---|
|/threads|Number of Download Threads|`/threads=1`|
|/output|Folder where the images are saved|`/output=./downloaded`|
|/sleep|Waiting period between downloads in Milliseconds|`/sleep=1000`|
