# StS2 RNG Fix

슬레이 더 스파이어 2의 **상관된 난수(correlated randomness) 편향**을 제거하는 모드입니다.

## 버그

슬더스2는 한 판마다 약 12개의 RNG 스트림(맵, 셔플, 카드 생성, 보상, niche 등)을 만듭니다. 각 스트림은 **단순 덧셈**으로 시드가 정해집니다:

```csharp
// RunRngSet.CreateRng
new Rng(runSeed + hash(스트림이름))   // -> new System.Random((int)그 시드)
```

`System.Random`의 **첫 출력은 시드에 거의 선형**입니다. 그래서 같은 `runSeed`에서 파생된 두 스트림(예: 맵 스트림과 고래밥 저주 스트림)의 **첫 뽑기가 서로 상관**됩니다.

무조건부로는 모든 게 균등해 보여서 발견하기 어렵습니다. 하지만 **관측 가능한 것으로 조건을 거는 순간**(시작 맵, 첫 보상 등) 다른 결과가 편향된 부분집합으로 붕괴합니다:

- **고래밥(Neow's Bones)** 저주가 시작 맵에 따라 달라집니다 — 어떤 시작에선 빚 ~54%, 다른 시작에선 몸부림 ~73%, 일부 저주는 거의 0%.
- **2막 변화 카드**가 고정된 하위 풀로 크게 쏠리고, 맵별로 확률이 다릅니다.

이는 슬더스1에서 문서화된 **"Correlated Randomness"** 버그와 같은 것입니다([forgottenarbiter 분석](https://forgottenarbiter.github.io/Correlated-Randomness/), 슬더스1 *RNG Fix* 모드의 배경). 슬더스2는 `hash(스트림이름)` 오프셋으로 완화하려 했지만, 작은/구조적 시드 차이에서도 첫-뽑기 상관이 남아 불충분합니다.

## 수정 방법

`Rng` 생성자에 Harmony 패치 1개를 걸어, 각 스트림 시드를 `System.Random`에 넣기 전에 **splitmix32 avalanche 해시**로 한 번 섞습니다:

```
System.Random( mix(runSeed + hash(스트림이름)) )
```

비트 완전 확산으로 스트림 간 시드가 통계적으로 독립이 되어 상관이 사라집니다. 개별 스트림은 여전히 완벽히 균등합니다.

**오프라인 검증(시드 200만 개):** 상관된 4분기 스트림으로 10분기 선택을 조건부로 보면 raw 시드에선 **31.8% / 0.0%** 로 쏠리지만, mix 적용 후엔 **항목당 9.9~10.1%** 로 돌아옵니다.

## 중요: 바뀌는 것과 안 바뀌는 것

- ✅ **결정성 유지.** 같은 시드는 여전히 항상 같은 판을 만듭니다. 모드 자체가 결정적 함수입니다.
- ⚠️ **바닐라 시드 호환은 깨짐.** 같은 시드라도 바닐라와 *다른*(편향 없는) 결과가 나오므로, 미적용 유저와의 시드 공유/일일 시드는 재현되지 않습니다.
- ⚠️ **멀티플레이:** 결과가 바닐라와 달라지므로 로비의 전원이 이 모드를 써야 합니다(아니면 desync). **싱글플레이 권장.**

## 설치

1. [Releases](../../releases)에서 `Sts2RngFix-vX.Y.Z.zip` 다운로드.
2. `Sts2RngFix/` 폴더를 슬더스2 `mods/` 폴더에 넣기:
   ```
   Slay the Spire 2/mods/Sts2RngFix/Sts2RngFix.dll
   Slay the Spire 2/mods/Sts2RngFix/Sts2RngFix.json
   ```
3. 게임 실행.

제거 없이 끄려면 환경 변수 `STS2_RNG_FIX_DISABLED=1` 설정.

## 소스 빌드

```
dotnet build -c Release
```

슬더스2가 설치돼 있어야 합니다(빌드가 `sts2.dll` / `0Harmony.dll` 자동 탐색). 빌드 시 DLL+매니페스트가 게임 `mods/Sts2RngFix/` 로 복사됩니다.

## 라이선스

MIT — [LICENSE](LICENSE) 참조.
