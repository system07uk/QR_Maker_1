using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QRCoder;

namespace QR_Maker_1
{
    public partial class Form1 : Form
    {
        // =========================
        // 구성 상수(중앙집중)
        // =========================
        private const bool WebDavUseHttps = true;      // Synology 권장: HTTPS
        private const int WebDavPortHttps = 5006;      // WebDAV Server(HTTPS)
        private const int WebDavPortHttp = 5005;       // WebDAV Server(HTTP)
        private const int QrSizePx = 500;              // QR 최종 렌더 크기(px)
        private const int PixelsPerModule = 10;        // QR 모듈 픽셀 크기

        public Form1()
        {
            InitializeComponent(); // 디자이너 설정 유지

            // 이벤트 연결 (디자이너 컨트롤 이름 사용)
            button1.Click += BtnSelectFile_Click;

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "L (7%)", "M (15%)", "Q (25%)", "H (30%)" });
            comboBox1.SelectedIndex = 3; // 산업 환경 권장: H (30%)
            comboBox1.SelectedIndexChanged += CboECC_SelectedIndexChanged;

            // CSV 대량 생성 버튼은 디자이너에서 추가 후 Click을 BtnBatchGenerate_Click에 연결하세요.
            // 예: buttonBatch.Click += BtnBatchGenerate_Click;
        }

        // -----------------------------
        // 파일 선택 → QR 생성/저장 + 미리보기
        // -----------------------------
        private void BtnSelectFile_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "QR로 만들 파일 선택 (경로를 QR 데이터로 사용)",
                CheckFileExists = true,
                Multiselect = false,
                Filter = "전체 파일 (*.*)|*.*"
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
                UpdateQrPreview(ofd.FileName); // 파일 선택 시 QR 생성
            }
        }

        // -----------------------------
        // ECC 변경 시 → 미리보기 갱신 + 파일 재저장
        // -----------------------------
        private void CboECC_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text) && File.Exists(textBox1.Text))
            {
                UpdateQrPreview(textBox1.Text);
            }
        }

        /// <summary>
        /// QR 생성 → 라벨(위: 파일명, 아래: 원래 경로+이름) 합성 → 미리보기 갱신 → PNG 저장
        /// </summary>
        private void UpdateQrPreview(string fullPath)
        {
            try
            {
                var ecc = ParseEcc(comboBox1.SelectedItem?.ToString());

                // QR 데이터: UNC → WebDAV(HTTPS, 5006) 변환 기본 적용
                string qrData = GetQrData(fullPath);

                // QR 생성 및 리사이즈
                using var qrBitmap = CreateQrBitmap(qrData, ecc, PixelsPerModule, drawQuietZones: true);
                using var resized = ResizeTo(qrBitmap, QrSizePx, QrSizePx);

                // 라벨: 위 = 파일명(확장자 제외), 아래 = 원래 입력 경로 + 이름(확장자 제거)
                string topText = Path.GetFileNameWithoutExtension(fullPath);
                string bottomText = BuildDisplayFullPath(fullPath, topText);

                using var labeled = AddLabels(resized, topText, bottomText);

                // 저장 경로: 원본 폴더 + 파일명_QR.png (확장자 제거)
                string folder = Path.GetDirectoryName(fullPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string baseName = Path.GetFileNameWithoutExtension(fullPath);
                string outPath = Path.Combine(folder, $"{SanitizeFileName(baseName)}_QR.png");

                labeled.Save(outPath, ImageFormat.Png);

                // 미리보기 갱신(이전 이미지 해제 후 교체)
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = new Bitmap(labeled);

                label1.Text = $"QR 생성 완료 (ECC: {comboBox1.SelectedItem}) → {outPath}";
            }
            catch (Exception ex)
            {
                label1.Text = $"오류: {ex.Message}";
            }
        }

        // -----------------------------
        // CSV 대량 생성
        //   - 1열: 이름(라벨 상단/저장 파일명)
        //   - 2열: 데이터(경로/문자열 → QR 데이터)
        //   - 단일 컬럼 CSV도 지원(경로만 → 이름 유추/자동 생성)
        //   - 하단 라벨: 항상 "원래 입력값(경로/문자열) + 이름(확장자 없음)" 표기
        // -----------------------------
        private void BtnBatchGenerate_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "CSV 파일 선택",
                Filter = "CSV 파일 (*.csv)|*.csv",
                CheckFileExists = true
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            using var fbd = new FolderBrowserDialog
            {
                Description = "QR 이미지를 저장할 폴더 선택"
            };
            if (fbd.ShowDialog(this) != DialogResult.OK) return;

            string[] lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
            int count = 0;
            var ecc = ParseEcc(comboBox1.SelectedItem?.ToString());
            var qrGen = new QRCodeGenerator(); // (성능상 재사용) — 필요 시 유지

            // 헤더 감지: 'Name,Path' 등
            int startIndex = (lines.Length > 0 && IsHeaderLine(lines[0])) ? 1 : 0;

            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = SplitFlexible(line);
                if (parts.Length == 0) continue;

                string name;
                string originalData;

                if (parts.Length >= 2)
                {
                    name = Unquote(parts[0]).Trim();
                    originalData = Unquote(parts[1]).Trim();
                }
                else
                {
                    // 단일 컬럼: 경로만 존재 → 이름을 경로에서 유추 (없으면 QR_####)
                    originalData = Unquote(parts[0]).Trim();
                    name = SuggestNameFromPath(originalData);
                    if (string.IsNullOrWhiteSpace(name))
                        name = $"QR_{(count + 1):D4}";
                }

                if (string.IsNullOrWhiteSpace(originalData)) continue;

                try
                {
                    // QR 데이터 변환(UNC → WebDAV(HTTPS))
                    string qrData = GetQrData(originalData);

                    // QR 생성
                    using QRCodeData qrDataObj = qrGen.CreateQrCode(qrData, ecc, forceUtf8: true, utf8BOM: false);
                    using var qrCode = new QRCode(qrDataObj);
                    using var bmp = qrCode.GetGraphic(PixelsPerModule, Color.Black, Color.White, drawQuietZones: true);
                    using var resized = ResizeTo(bmp, QrSizePx, QrSizePx);

                    // 라벨 텍스트: 상단 = 이름(확장자 제거), 하단 = 원래 경로 + 이름(확장자 제거)
                    string nameNoExt = Path.GetFileNameWithoutExtension(name);
                    string bottomText = BuildDisplayFullPath(originalData, nameNoExt);

                    using var labeled = AddLabels(resized, nameNoExt, bottomText);

                    // 저장 파일명: 확장자 제거 후 _QR.png
                    string cleanName = Path.GetFileNameWithoutExtension(name);
                    string outPath = Path.Combine(fbd.SelectedPath, $"{SanitizeFileName(cleanName)}_QR.png");
                    labeled.Save(outPath, ImageFormat.Png);

                    count++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"라인 처리 실패 (#{i + 1}): {line} / {ex.Message}");
                }
            }

            MessageBox.Show($"{count}개의 QR 코드가 생성되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // =========================
        // QR 데이터 생성 진입점 (정책 적용)
        // =========================
        private static string GetQrData(string input)
        {
            // 현재 정책: UNC → WebDAV(HTTPS 5006) 변환
            return ConvertUncToWebDav(input, useHttps: WebDavUseHttps, httpsPort: WebDavPortHttps, httpPort: WebDavPortHttp);
        }

        // -----------------------------
        // UNC → WebDAV URL 변환.
        //   - \\server\share\path → https://server:5006/share/path
        //   - 경로 세그먼트별 URL 인코딩(공백/한글 안전)
        //   - UNC가 아니면 원문 그대로 반환
        // -----------------------------
        private static string ConvertUncToWebDav(string input, bool useHttps = true, int httpsPort = 5006, int httpPort = 5005)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            // UNC 형태만 변환: \\서버\공유\...
            if (!input.StartsWith(@"\\"))
                return input;

            // UNC → host + path 추출
            string trimmed = input.TrimStart('\\');       // "server\share\path\file"
            string forward = trimmed.Replace("\\", "/");  // "server/share/path/file"

            int slashIdx = forward.IndexOf('/');
            if (slashIdx < 0)
            {
                // "\\server"만 있는 경우: 루트
                string scheme = useHttps ? "https" : "http";
                int port = useHttps ? httpsPort : httpPort;
                return $"{scheme}://{forward}:{port}/";
            }

            string host = forward.Substring(0, slashIdx);      // "server"
            string path = forward.Substring(slashIdx + 1);     // "share/path/file"

            string encodedPath = EncodePathSegments(path);     // 각 세그먼트 인코딩
            string schemeFinal = useHttps ? "https" : "http";
            int portFinal = useHttps ? httpsPort : httpPort;

            return $"{schemeFinal}://{host}:{portFinal}/{encodedPath}";
        }

        /// <summary>
        /// 경로 세그먼트별 URL 인코딩
        /// "a/b c/눈사람.jpg" → "a/b%20c/%EB%88%88%EC%82%AC%EB%9E%8C.jpg"
        /// </summary>
        private static string EncodePathSegments(string path)
        {
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("/", segments.Select(s => Uri.EscapeDataString(s)));
        }

        // -----------------------------
        // "경로 + 이름" 라벨 표시용 텍스트 생성
        //   - 파일 경로: 파일이 있는 폴더 + nameForFile
        //   - 폴더/문자열: 해당 값 + nameForFile
        //   - URL 등도 단순 결합(표시 목적)
        // -----------------------------
        private static string BuildDisplayFullPath(string pathOrFile, string nameForFile)
        {
            if (string.IsNullOrWhiteSpace(pathOrFile)) return nameForFile ?? "";

            // URL인 경우: 슬래시로 단순 결합
            if (IsUrl(pathOrFile))
            {
                string left = pathOrFile.TrimEnd('/');
                return $"{left}/{nameForFile}";
            }

            // 윈도우 경로 가정: 마지막 '\', '/' 제거
            string candidate = pathOrFile.TrimEnd('\\', '/');

            // 확장자가 있으면 파일로 취급 (UNC/로컬 파일 모두 대응)
            bool looksLikeFile = Path.HasExtension(candidate);

            string folder = looksLikeFile
                ? (Path.GetDirectoryName(candidate) ?? candidate)
                : candidate;

            // 폴더 + 이름(확장자 제거)
            return Path.Combine(folder, nameForFile ?? "");
        }

        private static bool IsUrl(string s)
        {
            return Uri.TryCreate(s, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        // -----------------------------
        // Helper: 헤더 감지
        // -----------------------------
        private static bool IsHeaderLine(string line)
        {
            var lower = (line ?? string.Empty).Trim().ToLowerInvariant();
            // 아주 단순한 감지: "name"과 "path"가 모두 포함되면 헤더로 간주
            return lower.Contains("name") && lower.Contains("path");
        }

        // -----------------------------
        // Helper: 유연한 Split (',', ';', '\t')
        // -----------------------------
        private static string[] SplitFlexible(string line)
        {
            if (line.Contains('\t')) return line.Split('\t');
            if (line.Contains(';')) return line.Split(';');
            return line.Split(',');
        }

        // -----------------------------
        // Helper: 따옴표 제거
        // -----------------------------
        private static string Unquote(string s)
        {
            s = s.Trim();
            if (s.Length >= 2 && s.StartsWith("\"") && s.EndsWith("\""))
                return s.Substring(1, s.Length - 2);
            return s;
        }

        // -----------------------------
        // Helper: 경로에서 이름 유추(단일 컬럼 CSV 대응)
        // -----------------------------
        private static string SuggestNameFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";

            string trimmed = path.TrimEnd('\\', '/');

            try
            {
                // 파일 경로인 경우: 파일명(확장자 제외)
                string fileNameNoExt = Path.GetFileNameWithoutExtension(trimmed);
                if (!string.IsNullOrWhiteSpace(fileNameNoExt))
                {
                    return fileNameNoExt;
                }

                // 폴더/URL 경로인 경우: 마지막 세그먼트
                char sep = trimmed.Contains('\\') ? '\\' : '/';
                var parts = trimmed.Split(sep).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                if (parts.Length >= 1) return parts.Last();

                return "";
            }
            catch
            {
                return "";
            }
        }

        private static QRCodeGenerator.ECCLevel ParseEcc(string? label)
        {
            return (label?.StartsWith("L") ?? false) ? QRCodeGenerator.ECCLevel.L :
                   (label?.StartsWith("Q") ?? false) ? QRCodeGenerator.ECCLevel.Q :
                   (label?.StartsWith("H") ?? false) ? QRCodeGenerator.ECCLevel.H :
                   QRCodeGenerator.ECCLevel.M;
        }

        private static Bitmap CreateQrBitmap(string data, QRCodeGenerator.ECCLevel ecc, int pixelsPerModule, bool drawQuietZones)
        {
            var qrGen = new QRCodeGenerator();
            using QRCodeData qrData = qrGen.CreateQrCode(data, ecc, forceUtf8: true, utf8BOM: false);
            using var qrCode = new QRCode(qrData);
            return qrCode.GetGraphic(pixelsPerModule, Color.Black, Color.White, drawQuietZones: drawQuietZones);
        }

        private static Bitmap ResizeTo(Bitmap src, int width, int height)
        {
            var dest = new Bitmap(width, height);
            dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
            using var g = Graphics.FromImage(dest);
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(src, new Rectangle(0, 0, width, height));
            return dest;
        }

        private static Bitmap AddLabels(Bitmap qr, string topText, string bottomText)
        {
            const int topLabelHeight = 50;
            const int bottomLabelHeight = 70;
            int totalHeight = qr.Height + topLabelHeight + bottomLabelHeight;

            var result = new Bitmap(qr.Width, totalHeight);
            using var g = Graphics.FromImage(result);
            g.Clear(Color.White);

            // QR 본문
            g.DrawImage(qr, new Rectangle(0, topLabelHeight, qr.Width, qr.Height));

            using var topBaseFont = new Font("Segoe UI", 18, FontStyle.Bold, GraphicsUnit.Pixel);
            using var bottomBaseFont = new Font("Segoe UI", 14, FontStyle.Regular, GraphicsUnit.Pixel);

            using var topFont = FitFont(g, topText, topBaseFont, qr.Width - 20, topLabelHeight - 8, minSize: 10);
            using var bottomFont = FitFont(g, bottomText, bottomBaseFont, qr.Width - 20, bottomLabelHeight - 8, minSize: 10);

            var sfCenter = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            };

            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.DrawString(topText, topFont, Brushes.Black, new RectangleF(10, 0, qr.Width - 20, topLabelHeight), sfCenter);
            g.DrawString(bottomText, bottomFont, Brushes.Black, new RectangleF(10, qr.Height + topLabelHeight, qr.Width - 20, bottomLabelHeight), sfCenter);

            return result;
        }

        private static Font FitFont(Graphics g, string text, Font baseFont, int maxWidth, int maxHeight, int minSize)
        {
            float size = baseFont.Size;

            while (true)
            {
                using var tf = new Font(baseFont.FontFamily, size, baseFont.Style, GraphicsUnit.Pixel);
                var measured = g.MeasureString(text, tf, new SizeF(maxWidth, maxHeight));
                if ((measured.Width <= maxWidth && measured.Height <= maxHeight) || size <= minSize)
                {
                    // 새 Font 인스턴스 반환(상위에서 using으로 해제되도록)
                    return new Font(baseFont.FontFamily, size, baseFont.Style, GraphicsUnit.Pixel);
                }
                size -= 1f;
            }
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray());
            return string.IsNullOrWhiteSpace(cleaned) ? "QR" : cleaned;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            pictureBox1.Image?.Dispose();
            base.OnFormClosed(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}