# UI 조화 검토 및 적용

기존 Start/Exit 버튼의 황토색·갈색, 전투 화면의 큼직한 픽셀, 지도 화면의 갈색·금색 패널 설정과 비교했다. 기존 1~4번보다 입체감과 장식을 줄인 각진 픽셀 로고를 새로 생성했다. 타이틀은 상단 중앙의 1100×366.6667 영역에 비율 유지로 배치하고 Point 필터, mipmap 없음, 무압축 스프라이트로 설정했다.

내장 image_gen 도구 사용. 참조: Assets/ThirdParty/ArtResource/JinWook/Button/Sprite_Start_Button.png

## 최종 프롬프트

Use case: logo-brand. Generate a FINAL usable transparent PNG game title logo. Reference image is the existing Start button of this game: match its coarse square pixel grid, muted ochre wood palette, dark brown flat outlines, humble charming 16-bit handmade UI. Text exactly "WORD VENTURE". W O R D above V E N T U R E, WORD smaller, VENTURE larger. All letters are blocky square pixel sans-serif forms, NOT serif fantasy lettering. Muted pale parchment and ochre flat letter faces, two simple shadow shades, dark warm brown outline; minimal deep green leaves at only two corners, a tiny book-page detail near W. Logo must feel like the same artist as reference button. Compact wide composition, aspect about 3:1, ample transparent margin, maximum readability at 550px width. Crisp chunky low-resolution pixel art, no smooth curves, no shiny gold bevels, no realistic texture, no glows, no scenery, no signboard, no additional text, no watermark. True transparent background. This is only the logo, not an entire screen.
