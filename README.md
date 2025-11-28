# QR Maker 1

QR 코드 생성 도구 (C# WinForms 앱). 로컬 파일, URL, UNC 경로, CSV 배치 입력을 지원하며, Synology WebDAV(HTTPS) 변환 기능 포함.

## 기능
- **단일 QR 생성**: 파일 선택 또는 텍스트/URL 입력으로 QR 코드 생성.
- **배치 생성**: CSV 파일에서 대량 QR 생성 (이름, 경로/데이터 지원).
- **ECC 수준 선택**: L(7%), M(15%), Q(25%), H(30%) (기본: H).
- **라벨 추가**: QR 아래에 파일명/경로 표시.
- **경로 변환**: UNC (`\\server\share`) → WebDAV URL (`https://server:5006/share`).
- **저장**: PNG 형식으로 자동 저장 (로컬/데스크톱).

## 요구사항
- .NET Framework 4.7.2 이상 (또는 .NET 6+).
- NuGet 패키지: `QRCoder` (QR 생성 라이브러리).

## 설치
1. GitHub에서 소스 코드 클론:
   ```
   git clone https://github.com/system07/QR_Maker_1.git
   ```
2. Visual Studio에서 프로젝트 열기 (`QR_Maker_1.sln`).
3. NuGet 패키지 복원: `Tools > NuGet Package Manager > Restore NuGet Packages`.
4. 빌드: `Build > Build Solution` (Release 모드 추천).

## 사용법
### 단일 모드
1. **파일 선택**: Button1 클릭 → 파일 선택 → 자동 QR 생성/미리보기/저장.
2. **URL/텍스트 입력**: textBox1에 URL(예: `https://example.com/file.txt`) 또는 경로 입력 → Button2 클릭 → QR 생성.
3. ECC 변경: ComboBox로 수준 선택 → 자동 갱신.

### 배치 모드
- CSV 형식: 
  - 2열: `Name,Path` (헤더 옵션).
  - 1열: Path만 (이름 자동 유추).
- ButtonBatch 클릭 → CSV 선택 → 저장 폴더 선택 → 생성.

## 구성
- WebDAV: HTTPS (포트 5006) 기본. 상수 수정 가능 (`Form1.cs`).
- QR 크기: 500x500px (PixelsPerModule=10).

## 문제 해결
- 오류: 콘솔 또는 label1 확인 (예: 경로 무효 시 데스크톱 fallback).
- 의존성: QRCoder 설치 확인.

## 라이선스
MIT License. 자세한 내용은 [LICENSE](LICENSE) 파일 참조.

---

*작성일: 2025-11-28 | 버전: 1.0*
