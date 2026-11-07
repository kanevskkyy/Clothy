# models/

Файли моделей у git не зберігаються. Скопіюйте їх із репозиторію graph-antifraud
(з кореня Clothy, PowerShell):

```powershell
$src = "D:\graph-antifraud\models"
$dst = "src\Clothy.AI.FraudService\models"
"service_main.json","service_device.json","encoders.json","service_config.yaml","ring_signal.yaml","fixture_synthetic.npz" |
  ForEach-Object { Copy-Item (Join-Path $src $_) $dst }
```

| Файл | Що це |
|---|---|
| `service_main.json` | XGBoost, 34 ознаки (`MODEL_VARIANT=main`) |
| `service_device.json` | XGBoost, 41 ознака з блоком пристрою (`MODEL_VARIANT=device`) |
| `encoders.json` | кодування категорій |
| `service_config.yaml` | порядок ознак, пороги з валідації, sha256 моделей |
| `ring_signal.yaml` | параметри сигналу кільця |
| `fixture_synthetic.npz` | фікстура для тестів |

Без цих файлів сервіс не стартує, а тести, яким вони потрібні, пропускаються (`skipif`).
