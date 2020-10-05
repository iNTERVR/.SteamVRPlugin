# Changelog
All notable changes to this package will be documented in this file.

## [1.4.9] - 2020-10-05

### Added

- Hand에 SetFocus(bool hasFocus)추가하여 시스템 이벤트 없이도 Focus 처리 가능하도록 기능 추가

## [1.4.8] - 2020-09-20

### Update

- Dependency: iNTERVR.IF.VR

## [1.4.7] - 2020-09-16

### Added

- Oculus와 같은 장비는 SteamVR 초기화에 실패하지만 실제 VR뷰를 제공하므로 Fallback하지 않도록 한다.

## [1.4.6] - 2020-09-12

### Update

- Dependency

### Changed

- IF_VR_Player의 [SteamVR]을 Rig의 루트로 이동하여 Fallback과 VR상황에 대응하도록 수정

## [1.4.5] - 2020-09-12

### Update

- Dependency

### Added

- Falloff camera controller가 활성활 될 때 Unity XR TrackedDevice 작동을 해제

## [1.4.3] - 2020-08-25

### Added

- IF_VR_Steam_Interactable에 Event/Delegate들 인자값 수정 보완

## [1.4.2] - 2020-08-25

### Added

- IF_VR_Steam_Interactable에 Event/Delegate들에 Interactable(this) 추가
- IF_VR_Steam_Interactable에 onUpdate Event/Delegate 추가

## [1.4.1] - 2020-08-25

### Added

- IF_VR_Steam_Interactable에 Event/Delegate (onHandHoverBegin, onHandHoverEnd, onDestroy, onDisable)들 추가

## [1.4.0] - 2020-08-22

### Changed

- Update dependency

## [1.3.0] - 2020-08-21

### Changed

- Update dependency

## [1.2.0] - 2020-08-21

### Fixed

- VR장비 문제 발생시 대체(fallback)처리 버그 수정

## [1.1.0] - 2020-08-18

### Changed

- ComponentBuilder의 BuildTracker함수에서 SteamVR_Behaviour_Pose컴포넌트를 찾을 때 활성화 된 컴포넌트만 찾는것으로 수정

## [1.0.0] - 2020-08-18

### Changed

- Change RC 1.0.0

## [0.3.0] - 2020-08-15

### Changed

- Edit README

## [0.2.0] - 2020-08-15

### Added

- GitHub action for npm-publish

## [0.1.0] - 2020-08-15

### Added

- Initial release
