import classNames from "classnames";
import { getModule } from "cs2/modding";
import { Theme } from "cs2/bindings";
import PickerIcon from "../../../images/Picker.png";
import { FormLine } from "../form-line/form-line";
import { VanillaComponentResolver } from "../vanilla-component/vanilla-components";
import {OnOpenPicker} from "../../bindings";

interface InfoSectionComponent {
	group: string;
	tooltipKeys: Array<string>;
	tooltipTags: Array<string>;
}

const InfoSectionTheme: Theme | any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.module.scss",
	"classes"
);

const InfoRowTheme: Theme | any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.module.scss",
	"classes"
)

const InfoSection: any = getModule( 
    "game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.tsx",
    "InfoSection"
)

const InfoRow: any = getModule(
    "game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.tsx",
    "InfoRow"
)

const DropdownStyle: Theme | any = getModule("game-ui/menu/themes/dropdown.module.scss", "classes");


const StationPylonRow = () => {
    return (
		<div>
					<FormLine title={"Add Upgrade"}>
						<div style={{ display: 'flex'}}>
							<VanillaComponentResolver.instance.ToolButton
								selected={true}
								multiSelect={false}
								src={PickerIcon}
								tooltip={"Pick Building"}
								className={classNames(
									VanillaComponentResolver.instance.toolButtonTheme.button
								  )}
								onSelect={() => { OnOpenPicker() }}
							/>
						</div>
					</FormLine>
			 </div>
    );
}
export const StationModuleSelectedInfoComponent = (componentList: any): any => {
	componentList["SubwayFreedomAssetPack.System.SelectedBuildingUISystem"] = (e: InfoSectionComponent) => {
		return <StationPylonRow />
	}
	return componentList as any;
}
